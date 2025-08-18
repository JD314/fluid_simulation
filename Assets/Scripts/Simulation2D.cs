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

    [Header("Legacy Obstacle Settings")]
    public Vector2 obstacleSize;
    public Vector2 obstacleCentre;
    public bool displayObstacle = true;
    
    [Header("Maze")]
    [HideInInspector] public List<ObstacleRectangle> obstacles = new List<ObstacleRectangle>();
    
    [Header("Obstacle File Settings")]
    public bool useMaze = true;
    public string obstacleFilePath = "mazes_csv/obstacles.csv";
    public bool loadObstaclesFromFile = true;
    public bool reloadObstaclesOnStart = true;
    

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
        // Cargar obstáculos desde archivo si está habilitado
        if (useMaze && loadObstaclesFromFile)
        {
            List<ObstacleRectangle> loadedObstacles = ObstacleLoader.LoadObstaclesFromCSV(obstacleFilePath);
            if (loadedObstacles.Count > 0)
            {
                obstacles = loadedObstacles;
                Debug.Log($"Se cargaron {obstacles.Count} obstáculos desde {obstacleFilePath}");
            }
            else
            {
                // Fallback a obstáculos de prueba si no se puede cargar el archivo
                Debug.LogWarning("No se pudieron cargar obstáculos desde el archivo, usando obstáculos de prueba");
                obstacles.Add(new ObstacleRectangle(new Vector2(0, 0), new Vector2(1, 1)));
                obstacles.Add(new ObstacleRectangle(new Vector2(5, 5), new Vector2(2, 1)));
            }
        }
        else
        {
            // Inicializar obstáculos de prueba si no se usa archivo
            if (obstacles.Count == 0)
            {
                obstacles.Add(new ObstacleRectangle(new Vector2(0, 0), new Vector2(1, 1)));
                obstacles.Add(new ObstacleRectangle(new Vector2(5, 5), new Vector2(2, 1)));
            }
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
        // Trabajar aquí de como se hara el obstaculo más complejo
        compute.SetVector("boundsSize", boundsSize);
        
        // Legacy obstacle settings (mantener para compatibilidad)
        compute.SetVector("obstacleSize", obstacleSize);
        compute.SetVector("obstacleCentre", obstacleCentre);
        
        // New obstacle system
        if (useMaze && obstacles.Count > 0)
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
    
    public void ReloadObstaclesFromFile()
    {
        if (loadObstaclesFromFile)
        {
            List<ObstacleRectangle> loadedObstacles = ObstacleLoader.LoadObstaclesFromCSV(obstacleFilePath);
            if (loadedObstacles.Count > 0)
            {
                obstacles = loadedObstacles;
                Debug.Log($"Se recargaron {obstacles.Count} obstáculos desde {obstacleFilePath}");
                
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


    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawWireCube(Vector2.zero, boundsSize);
        foreach (var obstacle in obstacles)
        {
            Gizmos.DrawWireCube(obstacle.position, obstacle.size);
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
