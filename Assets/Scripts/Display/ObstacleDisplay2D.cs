using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class ObstacleDisplay2D : MonoBehaviour
{
    public Shader obstacleShader;
    public Color obstacleColor = Color.white;
    public float worldScale = 20.0f;
    public float scaleY = 1.0f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;
    
    [Header("Maze")]
    public bool useMaze = true;
    public int maxObstacles = 100; // Máximo número de obstáculos que puede manejar el shader
    
    private Material material;
    private MeshRenderer meshRenderer;
    private Simulation2D simulation;
    private ComputeBuffer obstacleBuffer;
    
    void Awake()
    {
        Debug.Log("ObstacleDisplay2D: Awake called!");
    }
    
    void Start()
    {
        // Get simulation
        simulation = GetComponent<Simulation2D>();
        if (simulation == null)
        {
            simulation = FindObjectOfType<Simulation2D>();
            if (simulation == null)
            {
                Debug.LogError("No Simulation2D found in scene!");
                return;
            }
        }
        
        // Create material
        if (obstacleShader != null)
        {
            material = new Material(obstacleShader);
        }
        else
        {
            obstacleShader = Shader.Find("Custom/Obstacle2D");
            if (obstacleShader != null)
            {
                material = new Material(obstacleShader);
            }
            else
            {
                Debug.LogError("Obstacle2D shader not found!");
                return;
            }
        }
        
        // Set up the quad
        SetupQuad();
        
        // Set initial values
        material.SetColor("_Color", obstacleColor);
        material.SetFloat("_Scale", worldScale);
        material.SetFloat("_ScaleY", scaleY);
        material.SetFloat("_OffsetX", offsetX);
        material.SetFloat("_OffsetY", offsetY);
        
        // Initialize obstacle buffer for new system
        if (useMaze)
        {
            InitializeObstacleBuffer();
        }
    }
    
    void InitializeObstacleBuffer()
    {
        if (obstacleBuffer != null)
        {
            obstacleBuffer.Release();
        }
        
        obstacleBuffer = new ComputeBuffer(maxObstacles, sizeof(float) * 4); // float4: pos_x, pos_y, width, height
        material.SetBuffer("_Obstacles", obstacleBuffer);
        material.SetInt("_MaxObstacles", maxObstacles);
    }
    
    void SetupQuad()
    {
        // Create a quad mesh
        Mesh quadMesh = CreateQuadMesh();
        
        // Get or create MeshRenderer
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        // Get or create MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        meshFilter.mesh = quadMesh;
        meshRenderer.material = material;
    }
    
    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        
        // Create a large quad that covers the screen
        Vector3[] vertices = {
            new Vector3(-50, -50, 0),
            new Vector3(50, -50, 0),
            new Vector3(50, 50, 0),
            new Vector3(-50, 50, 0)
        };
        
        int[] triangles = {
            0, 1, 2,
            0, 2, 3
        };
        
        Vector2[] uvs = {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    void Update()
    {
        if (material != null && simulation != null)
        {
            // Check if obstacle should be displayed
            bool shouldDisplay = simulation.displayObstacle && 
                               (useMaze ? simulation.obstacles.Count > 0 : true);
            
            meshRenderer.enabled = shouldDisplay;
            
            if (shouldDisplay)
            {
                if (useMaze && simulation.useMaze)
                {
                    UpdateNewObstacleSystem();
                }
                else
                {
                    UpdateLegacyObstacleSystem();
                }
                
                // Update common properties
                material.SetFloat("_Scale", worldScale);
                material.SetFloat("_ScaleY", scaleY);
                material.SetFloat("_OffsetX", offsetX);
                material.SetFloat("_OffsetY", offsetY);
            }
        }
    }
    
    void UpdateNewObstacleSystem()
    {
        if (simulation.obstacles.Count > 0)
        {
            // Convert obstacles to float4 array
            float4[] obstacleData = new float4[Mathf.Min(simulation.obstacles.Count, maxObstacles)];
            for (int i = 0; i < obstacleData.Length; i++)
            {
                obstacleData[i] = new float4(
                    simulation.obstacles[i].position.x,
                    simulation.obstacles[i].position.y,
                    simulation.obstacles[i].size.x,
                    simulation.obstacles[i].size.y
                );
            }
            
            obstacleBuffer.SetData(obstacleData);
            material.SetInt("_NumObstacles", obstacleData.Length);
        }
        else
        {
            material.SetInt("_NumObstacles", 0);
        }
    }
    
    void UpdateLegacyObstacleSystem()
    {
        // Update legacy obstacle data
        material.SetVector("_ObstacleSize", simulation.obstacleSize);
        material.SetVector("_ObstacleCentre", simulation.obstacleCentre);
    }
    
    void OnDestroy()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
        
        if (obstacleBuffer != null)
        {
            obstacleBuffer.Release();
        }
    }
}
