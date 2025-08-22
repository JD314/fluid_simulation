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

[System.Serializable]
public struct ParticleTypeConfig
{
    [Header("Física")]
    public float gravity;
    public float targetDensity;
    public float pressureMultiplier;
    public float nearPressureMultiplier;
    public float viscosityStrength;
    public float mass;
    public float compressibility;
    
    [Header("Visual")]
    public Color particleColor;
    public float particleScale;
    
    [Header("Spawn")]
    public int particleCount;
    public Vector2 initialVelocity;
    public float jitterStrength;
    
    public static ParticleTypeConfig DefaultFluid()
    {
        return new ParticleTypeConfig
        {
            gravity = -12.0f,
            targetDensity = 55f,
            pressureMultiplier = 500f,
            nearPressureMultiplier = 18f,
            viscosityStrength = 0.06f,
            mass = 1.0f,
            compressibility = 1.0f,
            particleColor = Color.cyan,
            particleScale = 1.0f,
            particleCount = 800,
            initialVelocity = Vector2.zero,
            jitterStrength = 0.1f
        };
    }
    
    public static ParticleTypeConfig DefaultAir()
    {
        return new ParticleTypeConfig
        {
            gravity = 0.0f, // Sin gravedad para el aire
            targetDensity = 8f, // Mucho menos denso que el fluido
            pressureMultiplier = 150f, // Menos presión
            nearPressureMultiplier = 6f,
            viscosityStrength = 0.02f, // Menos viscoso
            mass = 0.2f, // Más ligero
            compressibility = 3.0f, // Más compresible
            particleColor = Color.white,
            particleScale = 0.6f,
            particleCount = 300,
            initialVelocity = Vector2.zero,
            jitterStrength = 0.05f
        };
    }
}

public class Simulation2D : MonoBehaviour
{
    public event System.Action SimulationStepCompleted;

    [Header("Simulation Settings")]
    public float timeScale = 1;
    public bool fixedTimeStep;
    public int iterationsPerFrame = 3;
    public float smoothingRadius = 0.35f;
    [Range(0, 1)] public float collisionDamping = 0.95f;
    public Vector2 boundsSize;
    
    [Header("Particle Types")]
    public ParticleTypeConfig fluidConfig = ParticleTypeConfig.DefaultFluid();
    public ParticleTypeConfig airConfig = ParticleTypeConfig.DefaultAir();
    
    [Header("Interaction Settings")]
    public float interactionRadius;
    public float interactionStrength;
    public float obstacleEdgeThreshold = 0.1f; // Threshold for particles to stick to obstacle edges
    public float fluidAirInteractionStrength = 0.3f; // Strength of interaction between fluid and air particles
    

    [Header("Maze")]
    [HideInInspector] public List<ObstacleRectangle> obstacles = new List<ObstacleRectangle>();
    [HideInInspector] public List<SpawnRegion> spawnRegions = new List<SpawnRegion>();
    
    [Header("Obstacle Settings")]
    public bool displayObstacle = true;
    public bool useMaze = true;
    public string obstacleFilePath = "mazes_csv/obstacles_with_air.csv";
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
    public ComputeBuffer typeBuffer { get; private set; } // Nuevo buffer para tipos de partículas
    public ComputeBuffer massBuffer { get; private set; } // Nuevo buffer para masas
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
    public int numFluidParticles { get; private set; }
    public int numAirParticles { get; private set; }

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

        // Calcular número total de partículas
        numFluidParticles = fluidConfig.particleCount;
        numAirParticles = airConfig.particleCount;
        numParticles = numFluidParticles + numAirParticles;

        // Generar datos de spawn para ambos tipos de partículas
        spawnData = GenerateMultiTypeSpawnData();

        // Create buffers
        positionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        predictedPositionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        velocityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        densityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
        typeBuffer = ComputeHelper.CreateStructuredBuffer<int>(numParticles); // 0 = fluido, 1 = aire
        massBuffer = ComputeHelper.CreateStructuredBuffer<float>(numParticles);
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
        ComputeHelper.SetBuffer(compute, typeBuffer, "ParticleTypes", externalForcesKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, massBuffer, "ParticleMasses", externalForcesKernel, densityKernel, pressureKernel, viscosityKernel);
        ComputeHelper.SetBuffer(compute, obstacleBuffer, "Obstacles", updatePositionKernel);

        compute.SetInt("numParticles", numParticles);
        compute.SetInt("numFluidParticles", numFluidParticles);
        compute.SetInt("numAirParticles", numAirParticles);
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
        compute.SetFloat("collisionDamping", collisionDamping);
        compute.SetFloat("smoothingRadius", smoothingRadius);
        compute.SetFloat("obstacleEdgeThreshold", obstacleEdgeThreshold);
        compute.SetFloat("obstacleOverlapOffset", obstacleOverlapOffset);
        compute.SetFloat("fluidAirInteractionStrength", fluidAirInteractionStrength);
        compute.SetVector("boundsSize", boundsSize);
        
        // Configuraciones específicas por tipo de partícula
        compute.SetFloat("fluidGravity", fluidConfig.gravity);
        compute.SetFloat("fluidTargetDensity", fluidConfig.targetDensity);
        compute.SetFloat("fluidPressureMultiplier", fluidConfig.pressureMultiplier);
        compute.SetFloat("fluidNearPressureMultiplier", fluidConfig.nearPressureMultiplier);
        compute.SetFloat("fluidViscosityStrength", fluidConfig.viscosityStrength);
        compute.SetFloat("fluidMass", fluidConfig.mass);
        compute.SetFloat("fluidCompressibility", fluidConfig.compressibility);
        
        compute.SetFloat("airGravity", airConfig.gravity);
        compute.SetFloat("airTargetDensity", airConfig.targetDensity);
        compute.SetFloat("airPressureMultiplier", airConfig.pressureMultiplier);
        compute.SetFloat("airNearPressureMultiplier", airConfig.nearPressureMultiplier);
        compute.SetFloat("airViscosityStrength", airConfig.viscosityStrength);
        compute.SetFloat("airMass", airConfig.mass);
        compute.SetFloat("airCompressibility", airConfig.compressibility);
        
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

    ParticleSpawner.ParticleSpawnData GenerateMultiTypeSpawnData()
    {
        var data = new ParticleSpawner.ParticleSpawnData(numParticles);
        var rng = new Unity.Mathematics.Random(42);

        // Generar partículas de fluido en la región de spawn original
        GenerateParticlesInRegion(data, 0, numFluidParticles, fluidConfig, spawner.spawnCentre, spawner.spawnSize, rng, 0);

        // Generar partículas de aire en las regiones de spawn del CSV con spawn por cuadrícula 1x1
        int airParticleIndex = numFluidParticles;
        foreach (var spawnRegion in spawnRegions)
        {
            int particlesInRegion = Mathf.Min(airConfig.particleCount / Mathf.Max(spawnRegions.Count, 1), 100); // Máximo 100 por región
            GenerateAirParticlesInGrid(data, airParticleIndex, particlesInRegion, airConfig, spawnRegion.position, spawnRegion.size, rng, 1);
            airParticleIndex += particlesInRegion;
        }

        // Si no hay regiones de spawn o no se generaron suficientes partículas de aire, generar en una región por defecto
        if (airParticleIndex < numParticles)
        {
            Vector2 defaultAirSpawnCentre = new Vector2(0, 20); // Región por defecto para aire
            Vector2 defaultAirSpawnSize = new Vector2(10, 5);
            int remainingAirParticles = numParticles - airParticleIndex;
            GenerateAirParticlesInGrid(data, airParticleIndex, remainingAirParticles, airConfig, defaultAirSpawnCentre, defaultAirSpawnSize, rng, 1);
        }

        return data;
    }

    void GenerateParticlesInRegion(ParticleSpawner.ParticleSpawnData data, int startIndex, int count, ParticleTypeConfig config, Vector2 centre, Vector2 size, Unity.Mathematics.Random rng, int particleType)
    {
        float2 s = size;
        int numX = Mathf.CeilToInt(Mathf.Sqrt(s.x / s.y * count + (s.x - s.y) * (s.x - s.y) / (4 * s.y * s.y)) - (s.x - s.y) / (2 * s.y));
        int numY = Mathf.CeilToInt(count / (float)numX);
        int i = 0;

        for (int y = 0; y < numY && startIndex + i < data.positions.Length; y++)
        {
            for (int x = 0; x < numX && startIndex + i < data.positions.Length; x++)
            {
                if (i >= count) break;

                float tx = numX <= 1 ? 0.5f : x / (numX - 1f);
                float ty = numY <= 1 ? 0.5f : y / (numY - 1f);

                float angle = (float)rng.NextDouble() * 3.14f * 2;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 jitter = dir * config.jitterStrength * ((float)rng.NextDouble() - 0.5f);
                
                int particleIndex = startIndex + i;
                data.positions[particleIndex] = new Vector2((tx - 0.5f) * size.x, (ty - 0.5f) * size.y) + jitter + centre;
                data.velocities[particleIndex] = config.initialVelocity;
                i++;
            }
        }
    }
    
    void GenerateAirParticlesInGrid(ParticleSpawner.ParticleSpawnData data, int startIndex, int count, ParticleTypeConfig config, Vector2 centre, Vector2 size, Unity.Mathematics.Random rng, int particleType)
    {
        // Generar partículas de aire en una cuadrícula 1x1 (una partícula por unidad cuadrada)
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        int i = 0;

        for (int y = 0; y < gridSize && startIndex + i < data.positions.Length; y++)
        {
            for (int x = 0; x < gridSize && startIndex + i < data.positions.Length; x++)
            {
                if (i >= count) break;

                // Calcular posición en la cuadrícula 1x1
                float tx = (x + 0.5f) / gridSize; // Centrar en cada celda
                float ty = (y + 0.5f) / gridSize;

                // Aplicar jitter mínimo para evitar partículas perfectamente alineadas
                float jitterX = ((float)rng.NextDouble() - 0.5f) * 0.1f;
                float jitterY = ((float)rng.NextDouble() - 0.5f) * 0.1f;
                
                int particleIndex = startIndex + i;
                data.positions[particleIndex] = new Vector2((tx - 0.5f) * size.x, (ty - 0.5f) * size.y) + new Vector2(jitterX, jitterY) + centre;
                data.velocities[particleIndex] = config.initialVelocity;
                i++;
            }
        }
    }

    void SetInitialBufferData(ParticleSpawner.ParticleSpawnData spawnData)
    {
        float2[] allPoints = new float2[spawnData.positions.Length];
        System.Array.Copy(spawnData.positions, allPoints, spawnData.positions.Length);

        positionBuffer.SetData(allPoints);
        predictedPositionBuffer.SetData(allPoints);
        velocityBuffer.SetData(spawnData.velocities);
        
        // Inicializar buffers de tipos y masas
        int[] particleTypes = new int[numParticles];
        float[] particleMasses = new float[numParticles];
        
        for (int i = 0; i < numFluidParticles; i++)
        {
            particleTypes[i] = 0; // Fluido
            particleMasses[i] = fluidConfig.mass;
        }
        
        for (int i = numFluidParticles; i < numParticles; i++)
        {
            particleTypes[i] = 1; // Aire
            particleMasses[i] = airConfig.mass;
        }
        
        typeBuffer.SetData(particleTypes);
        massBuffer.SetData(particleMasses);
        
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
        
        // Regenerar partículas si se cambian las configuraciones de partículas
        if (Application.isPlaying)
        {
            CancelInvoke("DelayedRegenerateParticles");
            Invoke("DelayedRegenerateParticles", 0.1f);
        }
    }
    
    /// <summary>
    /// Regeneración retardada de partículas para evitar múltiples llamadas
    /// </summary>
    void DelayedRegenerateParticles()
    {
        if (Application.isPlaying)
        {
            RegenerateParticles();
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
        ComputeHelper.Release(positionBuffer, predictedPositionBuffer, velocityBuffer, densityBuffer, typeBuffer, massBuffer, spatialIndices, spatialOffsets, obstacleBuffer);
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
    /// Regenera las partículas con las configuraciones actuales
    /// </summary>
    [ContextMenu("Regenerar Partículas")]
    public void RegenerateParticles()
    {
        // Recalcular número de partículas
        numFluidParticles = fluidConfig.particleCount;
        numAirParticles = airConfig.particleCount;
        numParticles = numFluidParticles + numAirParticles;
        
        // Regenerar datos de spawn
        spawnData = GenerateMultiTypeSpawnData();
        
        // Recrear buffers si es necesario
        if (positionBuffer != null && positionBuffer.count != numParticles)
        {
            ComputeHelper.Release(positionBuffer, predictedPositionBuffer, velocityBuffer, densityBuffer, typeBuffer, massBuffer);
            
            positionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            predictedPositionBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            velocityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            densityBuffer = ComputeHelper.CreateStructuredBuffer<float2>(numParticles);
            typeBuffer = ComputeHelper.CreateStructuredBuffer<int>(numParticles);
            massBuffer = ComputeHelper.CreateStructuredBuffer<float>(numParticles);
            
            // Reconfigurar buffers en el compute shader
            ComputeHelper.SetBuffer(compute, positionBuffer, "Positions", externalForcesKernel, updatePositionKernel);
            ComputeHelper.SetBuffer(compute, predictedPositionBuffer, "PredictedPositions", externalForcesKernel, spatialHashKernel, densityKernel, pressureKernel, viscosityKernel);
            ComputeHelper.SetBuffer(compute, velocityBuffer, "Velocities", externalForcesKernel, pressureKernel, viscosityKernel, updatePositionKernel);
            ComputeHelper.SetBuffer(compute, densityBuffer, "Densities", densityKernel, pressureKernel, viscosityKernel);
            ComputeHelper.SetBuffer(compute, typeBuffer, "ParticleTypes", externalForcesKernel, densityKernel, pressureKernel, viscosityKernel);
            ComputeHelper.SetBuffer(compute, massBuffer, "ParticleMasses", externalForcesKernel, densityKernel, pressureKernel, viscosityKernel);
            
            compute.SetInt("numParticles", numParticles);
            compute.SetInt("numFluidParticles", numFluidParticles);
            compute.SetInt("numAirParticles", numAirParticles);
            
            // Reconfigurar display
            if (display != null)
            {
                display.Init(this);
            }
        }
        
        SetInitialBufferData(spawnData);
        Debug.Log($"Partículas regeneradas: {numFluidParticles} fluido, {numAirParticles} aire");
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
