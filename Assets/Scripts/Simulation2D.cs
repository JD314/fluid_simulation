using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

[System.Serializable]
public struct ObstacleRectangle
{
    public Vector2 position; // Centro del rectángulo
    public Vector2 size;     // Ancho y alto del rectángulo
    
    public ObstacleRectangle(Vector2 pos, Vector2 s)
    {
        position = pos;
        size = s;
    }
}

[System.Serializable]
public struct SpawnRegion
{
    public Vector2 position;
    public Vector2 size;

    public SpawnRegion(Vector2 pos, Vector2 s)
    {
        position = pos;
        size = s;
    }
}

public class Simulation2D : MonoBehaviour
{
    public event System.Action SimulationStepCompleted;

    [Header("Simulation Settings")]
    public float timeScale = 1;
    public bool fixedTimeStep;
    public int iterationsPerFrame = 3;
    public float gravity = -12.0f;
    [Range(0, 1)] public float collisionDamping = 0.95f;
    public float smoothingRadius = 0.35f;
    public float targetDensity = 55f;
    public float pressureMultiplier = 500f;
    public float nearPressureMultiplier = 18f;
    public float viscosityStrength = 0.06f;
    public Vector2 boundsSize;
    
    [Header("Interaction Settings")]
    public float interactionRadius;
    public float interactionStrength;
    public float obstacleEdgeThreshold = 0.1f; // Threshold for particles to stick to obstacle edges
    

    [Header("Maze")]
    [HideInInspector] public List<ObstacleRectangle> obstacles = new List<ObstacleRectangle>();
    [HideInInspector] public List<SpawnRegion> spawnRegions = new List<SpawnRegion>();
    
    [Header("Obstacle Settings")]
    public bool displayObstacle = true;
    public bool useMaze = true;
    public string obstacleFilePath = "mazes_csv/obstacles.csv";
    public float obstacleOverlapOffset = 0.05f; // Small overlap between obstacles to prevent particle leakage
    public bool loadObstaclesFromFile = true;
    public bool reloadObstaclesOnStart = true;
    public bool displayObstacleHitbox = true;
    
    [Header("Horizontal Barriers")]
    public bool enableHorizontalBarriers = true;
    public float barrierWidth = 10.0f;
    public float barrierHeight = 0.3f;
    public float barrierSpacing = 20.0f;
    public float barrierStartY = 30.0f;
    

    [Header("References")]
    public ComputeShader compute;
    public ParticleSpawner spawner;
    public ParticleDisplay2D display;
    

    // Buffers
    public ComputeBuffer positionBuffer { get; private set; }
    public ComputeBuffer velocityBuffer { get; private set; }
    public ComputeBuffer densityBuffer { get; private set; }
    ComputeBuffer predictedPositionBuffer;
    ComputeBuffer spatialIndices;
    ComputeBuffer spatialOffsets;
    ComputeBuffer obstacleBuffer; // Nuevo buffer para obstáculos
    GPUSort gpuSort;

    // Kernel IDs
    const int externalForcesKernel = 0;
    const int spatialHashKernel = 1;
    const int densityKernel = 2;
    const int pressureKernel = 3;
    const int viscosityKernel = 4;
    const int updatePositionKernel = 5;

    // State
    bool isPaused;
    ParticleSpawner.ParticleSpawnData spawnData;
    bool pauseNextFrame;

    public int numParticles { get; private set; }

    void Awake()
    {
        // Cargar laberinto (obstáculos y spawns) desde archivo si está habilitado
        if (useMaze && loadObstaclesFromFile)
        {
            var mazeData = ObstacleLoader.LoadMazeFromCSV(obstacleFilePath);
            if (mazeData.obstacles.Count > 0 || mazeData.spawns.Count > 0)
            {
                obstacles = mazeData.obstacles;
                spawnRegions = mazeData.spawns;
                Debug.Log($"Se cargaron {obstacles.Count} obstáculos y {spawnRegions.Count} spawns desde {obstacleFilePath}");
            }
            else
            {
                Debug.LogWarning("No se pudieron cargar obstáculos desde el archivo, pero useMaze está activado");
            }
        }
        
        // Agregar barras horizontales si están habilitadas (independientemente del laberinto)
        if (enableHorizontalBarriers)
        {
            AddDefaultHorizontalBarriers();
        }
    }
    
    /// <summary>
    /// Agrega las barras horizontales por defecto
    /// </summary>
    private void AddDefaultHorizontalBarriers()
    {
        if (enableHorizontalBarriers)
        {
            AddHorizontalBarrierAtY(barrierStartY);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing * 2);
            Debug.Log($"Barras horizontales por defecto agregadas en Y={barrierStartY}, {barrierStartY + barrierSpacing}, {barrierStartY + barrierSpacing * 2}");
        }
    }

    void Start()
    {
        Debug.Log("Controls: Space = Play/Pause, R = Reset, O = Reload Obstacles, P = Save Obstacles, LMB = Attract, RMB = Repel");

        float deltaTime = 1 / 60f;
        Time.fixedDeltaTime = deltaTime;

        spawnData = spawner.GetSpawnData();
        numParticles = spawnData.positions.Length;

        // Create buffers
        positionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        predictedPositionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        velocityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        densityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        spatialIndices = ComputeHelper.CreateStructuredBuffer<uint3>(numParticles);
        spatialOffsets = ComputeHelper.CreateStructuredBuffer<uint>(numParticles);
        obstacleBuffer = ComputeHelper.CreateStructuredBuffer<float4>(Mathf.Max(obstacles.Count, 1));

        // Set buffer data
        SetInitialBufferData(spawnData);

        // Init compute
        ComputeHelper.SetBuffer(compute, positionBuffer, "Positions", externalForcesKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, predictedPositionBuffer, "PredictedPositions", externalForcesKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, spatialIndices, "SpatialIndices", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, spatialOffsets, "SpatialOffsets", spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, densityBuffer, "Densities", densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, velocityBuffer, "Velocities", externalForcesKernel, pressureKernel, viscosityKernel, updatePositionKernel);
        ComputeHelper.SetBuffer(compute, obstacleBuffer, "Obstacles", updatePositionKernel);

        compute.SetInt("numParticles", numParticles);
        compute.SetInt("numObstacles", obstacles.Count);

        gpuSort = new();
        gpuSort.SetBuffers(spatialIndices, spatialOffsets);


        // Init display
        display.Init(this);
    }

    void FixedUpdate()
    {
        if (fixedTimeStep)
        {
            RunSimulationFrame(Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        // Run simulation if not in fixed timestep mode
        // (skip running for first few frames as deltaTime can be disproportionaly large)
        if (!fixedTimeStep && Time.frameCount > 10)
        {
            RunSimulationFrame(Time.deltaTime);
        }

        if (pauseNextFrame)
        {
            isPaused = true;
            pauseNextFrame = false;
        }

        HandleInput();
    }

    void RunSimulationFrame(float frameTime)
    {
        if (!isPaused)
        {
            float timeStep = frameTime / iterationsPerFrame * timeScale;

            UpdateSettings(timeStep);

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                RunSimulationStep();
                SimulationStepCompleted?.Invoke();
            }
        }
    }

    void RunSimulationStep()
    {
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: externalForcesKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: spatialHashKernel);
        gpuSort.SortAndCalculateOffsets();
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: densityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: pressureKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: viscosityKernel);
        ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updatePositionKernel);

    }

    void UpdateSettings(float deltaTime)
    {
        compute.SetFloat("deltaTime", deltaTime);
        compute.SetFloat("gravity", gravity);
        compute.SetFloat("collisionDamping", collisionDamping);
        compute.SetFloat("smoothingRadius", smoothingRadius);
        compute.SetFloat("targetDensity", targetDensity);
        compute.SetFloat("pressureMultiplier", pressureMultiplier);
        compute.SetFloat("nearPressureMultiplier", nearPressureMultiplier);
        compute.SetFloat("viscosityStrength", viscosityStrength);
        compute.SetFloat("obstacleEdgeThreshold", obstacleEdgeThreshold);
        compute.SetFloat("obstacleOverlapOffset", obstacleOverlapOffset);
        // Trabajar aquí de como se hara el obstaculo más complejo
        compute.SetVector("boundsSize", boundsSize);
        

        
        // Update obstacle system (both maze and custom obstacles)
        if (obstacles.Count > 0)
        {
            // Convert ObstacleRectangle to float4 array for GPU
            float4[] obstacleData = new float4[obstacles.Count];
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacleData[i] = new float4(
                    obstacles[i].position.x,
                    obstacles[i].position.y,
                    obstacles[i].size.x,
                    obstacles[i].size.y
                );
            }
            obstacleBuffer.SetData(obstacleData);
            compute.SetInt("numObstacles", obstacles.Count);
        }
        else
        {
            compute.SetInt("numObstacles", 0);
        }

        compute.SetFloat("Poly6ScalingFactor", 4 / (Mathf.PI * Mathf.Pow(smoothingRadius, 8)));
        compute.SetFloat("SpikyPow3ScalingFactor", 10 / (Mathf.PI * Mathf.Pow(smoothingRadius, 5)));
        compute.SetFloat("SpikyPow2ScalingFactor", 6 / (Mathf.PI * Mathf.Pow(smoothingRadius, 4)));
        compute.SetFloat("SpikyPow3DerivativeScalingFactor", 30 / (Mathf.Pow(smoothingRadius, 5) * Mathf.PI));
        compute.SetFloat("SpikyPow2DerivativeScalingFactor", 12 / (Mathf.Pow(smoothingRadius, 4) * Mathf.PI));

        // // Mouse interaction settings:
        // Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // bool isPullInteraction = Input.GetMouseButton(0);
        // bool isPushInteraction = Input.GetMouseButton(1);
        // float currInteractStrength = 0;
        // if (isPushInteraction || isPullInteraction)
        // {
        //     currInteractStrength = isPushInteraction ? -interactionStrength : interactionStrength;
        // }

        // compute.SetVector("interactionInputPoint", mousePos);
        // compute.SetFloat("interactionInputStrength", currInteractStrength);
        // compute.SetFloat("interactionInputRadius", interactionRadius);
    }

    void SetInitialBufferData(ParticleSpawner.ParticleSpawnData spawnData)
    {
        float2[] allPoints = new float2[spawnData.positions.Length];
        System.Array.Copy(spawnData.positions, allPoints, spawnData.positions.Length);

        positionBuffer.SetData(allPoints);
        predictedPositionBuffer.SetData(allPoints);
        velocityBuffer.SetData(spawnData.velocities);
        
        // Initialize obstacle buffer
        if (obstacles.Count > 0)
        {
            float4[] obstacleData = new float4[obstacles.Count];
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacleData[i] = new float4(
                    obstacles[i].position.x,
                    obstacles[i].position.y,
                    obstacles[i].size.x,
                    obstacles[i].size.y
                );
            }
            obstacleBuffer.SetData(obstacleData);
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isPaused = false;
            pauseNextFrame = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isPaused = true;
            // Reset positions, the run single frame to get density etc (for debug purposes) and then reset positions again
            SetInitialBufferData(spawnData);
            RunSimulationStep();
            SetInitialBufferData(spawnData);
        }
        
        // Recargar obstáculos desde archivo
        if (Input.GetKeyDown(KeyCode.O))
        {
            ReloadObstaclesFromFile();
        }
        
        // Guardar obstáculos actuales a archivo
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveObstaclesToFile();
        }
    }
    
    public void HandleQuickActions()
    {
        // Add horizontal barriers if enabled (regardless of maze setting)
        if (enableHorizontalBarriers)
        {
            AddHorizontalBarrierAtY(barrierStartY);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing * 2);
            
            // Update the obstacle buffer
            UpdateObstacleBuffer();
            Debug.Log($"Barras horizontales agregadas en Y={barrierStartY}, {barrierStartY + barrierSpacing}, {barrierStartY + barrierSpacing * 2}");
        }
    }
    
    /// <summary>
    /// Actualiza las barras horizontales (llamar desde Inspector cuando cambies configuración)
    /// </summary>
    [ContextMenu("Actualizar Barras Horizontales")]
    public void UpdateHorizontalBarriers()
    {
        HandleQuickActions();
    }
    
    /// <summary>
    /// Se ejecuta cuando se cambian valores en el Inspector
    /// </summary>
    void OnValidate()
    {
        // Si estamos en modo de juego y las barras están habilitadas, actualizar automáticamente
        if (Application.isPlaying && enableHorizontalBarriers)
        {
            // Usar Invoke para evitar llamadas múltiples durante OnValidate
            CancelInvoke("DelayedUpdateBarriers");
            Invoke("DelayedUpdateBarriers", 0.1f);
        }
    }
    
    /// <summary>
    /// Actualización retardada de barras para evitar múltiples llamadas
    /// </summary>
    void DelayedUpdateBarriers()
    {
        if (enableHorizontalBarriers)
        {
            // Limpiar barras horizontales existentes (mantener obstáculos del laberinto)
            RemoveHorizontalBarriers();
            
            // Agregar nuevas barras con los parámetros actualizados
            AddHorizontalBarrierAtY(barrierStartY);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing * 2);
            
            // Actualizar el buffer
            UpdateObstacleBuffer();
            Debug.Log($"Barras horizontales actualizadas: Y={barrierStartY}, {barrierStartY + barrierSpacing}, {barrierStartY + barrierSpacing * 2}, W={barrierWidth}, H={barrierHeight}");
        }
    }
    
    /// <summary>
    /// Remueve solo las barras horizontales (mantiene obstáculos del laberinto)
    /// </summary>
    private void RemoveHorizontalBarriers()
    {
        // Remover barras horizontales basándose en su posición Y
        obstacles.RemoveAll(obstacle => 
            Mathf.Abs(obstacle.position.y - barrierStartY) < 1f ||
            Mathf.Abs(obstacle.position.y - (barrierStartY + barrierSpacing)) < 1f ||
            Mathf.Abs(obstacle.position.y - (barrierStartY + barrierSpacing * 2)) < 1f
        );
    }
    
    /// <summary>
    /// Activa o desactiva las barras horizontales dinámicamente
    /// </summary>
    [ContextMenu("Toggle Barras Horizontales")]
    public void ToggleHorizontalBarriers()
    {
        enableHorizontalBarriers = !enableHorizontalBarriers;
        
        if (enableHorizontalBarriers)
        {
            DelayedUpdateBarriers();
        }
        else
        {
            RemoveHorizontalBarriers();
            UpdateObstacleBuffer();
            Debug.Log("Barras horizontales desactivadas");
        }
    }
    
    public void ReloadObstaclesFromFile()
    {
        if (loadObstaclesFromFile)
        {
            var mazeData = ObstacleLoader.LoadMazeFromCSV(obstacleFilePath);
            if (mazeData.obstacles.Count > 0 || mazeData.spawns.Count > 0)
            {
                obstacles = mazeData.obstacles;
                spawnRegions = mazeData.spawns;
                Debug.Log($"Se recargaron {obstacles.Count} obstáculos y {spawnRegions.Count} spawns desde {obstacleFilePath}");
                
                // Recrear el buffer de obstáculos con el nuevo tamaño
                if (obstacleBuffer != null)
                {
                    obstacleBuffer.Release();
                }
                obstacleBuffer = ComputeHelper.CreateStructuredBuffer<float4>(Mathf.Max(obstacles.Count, 1));
                ComputeHelper.SetBuffer(compute, obstacleBuffer, "Obstacles", updatePositionKernel);
                
                // Reinicializar los datos del buffer
                SetInitialBufferData(spawnData);
            }
        }
    }
    
    public void SaveObstaclesToFile()
    {
        ObstacleLoader.SaveObstaclesToCSV(obstacles, obstacleFilePath);
    }


    void OnDestroy()
    {
        ComputeHelper.Release(positionBuffer, predictedPositionBuffer, velocityBuffer, densityBuffer, spatialIndices, spatialOffsets, obstacleBuffer);
    }
    
    // ===== OBSTACLE SETTINGS - FUNCIONES PERSONALIZADAS =====
    
    /// <summary>
    /// Agrega un obstáculo personalizado en la posición y tamaño especificados
    /// </summary>
    /// <param name="position">Posición del centro del obstáculo (Vector2)</param>
    /// <param name="width">Ancho del obstáculo</param>
    /// <param name="height">Alto del obstáculo</param>
    public void AddCustomObstacle(Vector2 position, float width, float height)
    {
        ObstacleRectangle newObstacle = new ObstacleRectangle(position, new Vector2(width, height));
        obstacles.Add(newObstacle);
        
        // Recrear el buffer de obstáculos con el nuevo tamaño
        if (obstacleBuffer != null)
        {
            obstacleBuffer.Release();
        }
        obstacleBuffer = ComputeHelper.CreateStructuredBuffer<float4>(Mathf.Max(obstacles.Count, 1));
        ComputeHelper.SetBuffer(compute, obstacleBuffer, "Obstacles", updatePositionKernel);
        
        // Forzar actualización inmediata del buffer
        UpdateObstacleBuffer();
        
        // Asegurar que el compute shader sepa que hay obstáculos
        compute.SetInt("numObstacles", obstacles.Count);
        
        Debug.Log($"Obstáculo agregado en posición {position} con tamaño {width}x{height}. Total de obstáculos: {obstacles.Count}");
    }
    
    /// <summary>
    /// Agrega múltiples obstáculos personalizados
    /// </summary>
    /// <param name="obstacleData">Array de datos de obstáculos (posición, ancho, alto)</param>
    public void AddMultipleObstacles(params (Vector2 position, float width, float height)[] obstacleData)
    {
        foreach (var data in obstacleData)
        {
            AddCustomObstacle(data.position, data.width, data.height);
        }
    }
    
    /// <summary>
    /// Limpia todos los obstáculos actuales
    /// </summary>
    public void ClearAllObstacles()
    {
        obstacles.Clear();
        UpdateObstacleBuffer();
        Debug.Log("Todos los obstáculos han sido eliminados");
    }
    
    /// <summary>
    /// Verifica el estado de los obstáculos (para debug)
    /// </summary>
    [ContextMenu("Verificar Estado de Obstáculos")]
    public void DebugObstacles()
    {
        Debug.Log($"Total de obstáculos en lista: {obstacles.Count}");
        Debug.Log($"Buffer de obstáculos existe: {obstacleBuffer != null}");
        Debug.Log($"Buffer de obstáculos count: {(obstacleBuffer != null ? obstacleBuffer.count : 0)}");
        Debug.Log($"Use Maze: {useMaze}");
        Debug.Log($"Load Obstacles From File: {loadObstaclesFromFile}");
        Debug.Log($"Obstacle File Path: {obstacleFilePath}");
        
        for (int i = 0; i < obstacles.Count; i++)
        {
            Debug.Log($"Obstáculo {i}: Pos={obstacles[i].position}, Size={obstacles[i].size}");
        }
    }
    
    /// <summary>
    /// Verifica el estado de las partículas (para debug)
    /// </summary>
    [ContextMenu("Verificar Estado de Partículas")]
    public void DebugParticles()
    {
        Debug.Log($"Número de partículas: {numParticles}");
        Debug.Log($"Spawner existe: {spawner != null}");
        Debug.Log($"Display existe: {display != null}");
        Debug.Log($"Simulación pausada: {isPaused}");
        Debug.Log($"Fixed Time Step: {fixedTimeStep}");
        Debug.Log($"Time Scale: {timeScale}");
        
        if (spawner != null)
        {
            Debug.Log($"Spawn Centre: {spawner.spawnCentre}");
            Debug.Log($"Spawn Size: {spawner.spawnSize}");
            Debug.Log($"Particle Count: {spawner.particleCount}");
            Debug.Log($"Initial Velocity: {spawner.initialVelocity}");
        }
        
        if (spawnData.positions != null && spawnData.positions.Length > 0)
        {
            Debug.Log($"Primera partícula posición: {spawnData.positions[0]}");
            Debug.Log($"Última partícula posición: {spawnData.positions[spawnData.positions.Length - 1]}");
        }
    }
    
    /// <summary>
    /// Resetea las partículas a su posición inicial
    /// </summary>
    [ContextMenu("Reset Partículas")]
    public void ResetParticles()
    {
        if (spawnData.positions != null)
        {
            SetInitialBufferData(spawnData);
            Debug.Log("Partículas reseteadas a posición inicial");
        }
    }
    
    /// <summary>
    /// Fuerza la recarga del laberinto
    /// </summary>
    [ContextMenu("Recargar Laberinto")]
    public void ReloadMaze()
    {
        if (useMaze && loadObstaclesFromFile)
        {
            var mazeData = ObstacleLoader.LoadMazeFromCSV(obstacleFilePath);
            if (mazeData.obstacles.Count > 0 || mazeData.spawns.Count > 0)
            {
                obstacles = mazeData.obstacles;
                spawnRegions = mazeData.spawns;
                
                // Agregar barras horizontales si están habilitadas
                if (enableHorizontalBarriers)
                {
                    AddDefaultHorizontalBarriers();
                }
                
                UpdateObstacleBuffer();
                Debug.Log($"Laberinto recargado: {obstacles.Count} obstáculos y {spawnRegions.Count} spawns");
            }
            else
            {
                Debug.LogError($"No se pudieron cargar obstáculos desde {obstacleFilePath}");
            }
        }
    }
    
    /// <summary>
    /// Agrega barras horizontales a los obstáculos existentes
    /// </summary>
    [ContextMenu("Agregar Barras Horizontales")]
    public void AddHorizontalBarriersToExisting()
    {
        if (enableHorizontalBarriers)
        {
            AddHorizontalBarrierAtY(barrierStartY);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing);
            AddHorizontalBarrierAtY(barrierStartY + barrierSpacing * 2);
            UpdateObstacleBuffer();
            Debug.Log($"Barras horizontales agregadas a obstáculos existentes. Total: {obstacles.Count}");
        }
    }
    
    /// <summary>
    /// Actualiza el buffer de obstáculos en el GPU
    /// </summary>
    private void UpdateObstacleBuffer()
    {
        if (obstacles.Count > 0 && obstacleBuffer != null)
        {
            float4[] obstacleData = new float4[obstacles.Count];
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacleData[i] = new float4(
                    obstacles[i].position.x,
                    obstacles[i].position.y,
                    obstacles[i].size.x,
                    obstacles[i].size.y
                );
            }
            obstacleBuffer.SetData(obstacleData);
            compute.SetInt("numObstacles", obstacles.Count);
            
            Debug.Log($"Buffer de obstáculos actualizado con {obstacles.Count} obstáculos");
        }
        else
        {
            compute.SetInt("numObstacles", 0);
        }
    }
    

    
    /// <summary>
    /// Agrega una barrera horizontal en la posición Y especificada
    /// </summary>
    /// <param name="yPosition">Posición Y de la barrera</param>
    private void AddHorizontalBarrierAtY(float yPosition)
    {
        Vector2 position = new Vector2(0, yPosition);
        ObstacleRectangle barrier = new ObstacleRectangle(position, new Vector2(barrierWidth, barrierHeight));
        obstacles.Add(barrier);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawWireCube(Vector2.zero, boundsSize);
        
        // Draw obstacle hitboxes only if enabled
        if ((useMaze && displayObstacleHitbox) || (!useMaze && displayObstacle))
        {
            foreach (var obstacle in obstacles)
            {
                // Draw the actual obstacle size
                Gizmos.color = new Color(0, 1, 0, 0.4f);
                Gizmos.DrawWireCube(obstacle.position, obstacle.size);
                
                // Draw the expanded obstacle size with overlap offset
                Gizmos.color = new Color(1, 0, 0, 0.2f); // Red for expanded collision area
                Vector2 expandedSize = obstacle.size + Vector2.one * obstacleOverlapOffset * 2.0f;
                Gizmos.DrawWireCube(obstacle.position, expandedSize);
            }
        }

        if (Application.isPlaying)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool isPullInteraction = Input.GetMouseButton(0);
            bool isPushInteraction = Input.GetMouseButton(1);
            bool isInteracting = isPullInteraction || isPushInteraction;
            if (isInteracting)
            {
                Gizmos.color = isPullInteraction ? Color.green : Color.red;
                Gizmos.DrawWireSphere(mousePos, interactionRadius);
            }
        }

    }
}
