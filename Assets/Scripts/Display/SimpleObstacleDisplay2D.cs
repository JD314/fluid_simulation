using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class SimpleObstacleDisplay2D : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color obstacleColor = Color.white; // Cambiado a blanco para mejor visibilidad
    public float scaleY = 1.0f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;
    
    [Header("Quad Settings")]
    public Vector2 sizeAdjustment = new Vector2(0, 0); // Ajuste por defecto: 0,0 para funcionamiento normal
    public Vector2 defaultPadding = new Vector2(40, 50); // Padding aumentado: más en X y aún más en Y
    
    [Header("References")]
    public Simulation2D simulation;
    
    private Material material;
    private MeshRenderer meshRenderer;
    private ComputeBuffer obstacleBuffer;
    private const int MAX_OBSTACLES = 2000;
    
    void Start()
    {
        // Get simulation reference
        if (simulation == null)
        {
            simulation = GetComponent<Simulation2D>();
            if (simulation == null)
            {
                simulation = FindObjectOfType<Simulation2D>();
                if (simulation == null)
                {
                    Debug.LogError("SimpleObstacleDisplay2D: No Simulation2D found!");
                    return;
                }
            }
        }
        
        // Create material with shader
        CreateMaterial();
        
        // Setup the display quad
        SetupDisplayQuad();
        
        // Initialize obstacle buffer
        InitializeObstacleBuffer();
    }
    
    void CreateMaterial()
    {
        Shader shader = Shader.Find("Custom/Obstacle2D");
        if (shader == null)
        {
            Debug.LogError("SimpleObstacleDisplay2D: Obstacle2D shader not found!");
            return;
        }
        
        material = new Material(shader);
        material.SetColor("_Color", obstacleColor);
        material.SetFloat("_ScaleY", scaleY);
        
        // Set initial quad size
        UpdateQuadScale();
    }
    
    void UpdateQuadScale()
    {
        if (material != null && simulation != null)
        {
            Vector2 quadSize = GetQuadSize();
            material.SetVector("_QuadSize", quadSize);
            
            // Calculate scale factor based on sizeAdjustment
            Vector2 baseSize = simulation.boundsSize;
            Vector2 adjustedSize = baseSize + sizeAdjustment;
            Vector2 scaleFactor = new Vector2(
                adjustedSize.x / baseSize.x,
                adjustedSize.y / baseSize.y
            );
            material.SetVector("_ScaleFactor", scaleFactor);
        }
    }
    
    void SetupDisplayQuad()
    {
        // Create a quad that matches the simulation bounds
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
        
        // Apply initial offset to transform
        UpdateQuadPosition();
    }
    
    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        
        // Determine the size to use for the quad
        Vector2 size = GetQuadSize();
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        
        // Create a quad with the determined size
        Vector3[] vertices = {
            new Vector3(-halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, halfHeight, 0),
            new Vector3(-halfWidth, halfHeight, 0)
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
    
    Vector2 GetQuadSize()
    {
        if (simulation != null)
        {
            // Use boundsSize + sizeAdjustment + defaultPadding as the canvas size
            return simulation.boundsSize + sizeAdjustment + defaultPadding;
        }
        else
        {
            // Fallback to default size + sizeAdjustment + defaultPadding
            return new Vector2(20, 20) + sizeAdjustment + defaultPadding;
        }
    }
    
    void InitializeObstacleBuffer()
    {
        if (obstacleBuffer != null)
        {
            obstacleBuffer.Release();
        }
        
        obstacleBuffer = new ComputeBuffer(MAX_OBSTACLES, sizeof(float) * 4); // float4: pos_x, pos_y, width, height
        material.SetBuffer("_Obstacles", obstacleBuffer);
        material.SetInt("_MaxObstacles", MAX_OBSTACLES);
    }
    
    void Update()
    {
        if (material == null || simulation == null) return;
        
        // Check if we should display obstacles
        bool shouldDisplay = simulation.displayObstacle && simulation.obstacles.Count > 0;
        meshRenderer.enabled = shouldDisplay;
        
        if (shouldDisplay)
        {
            UpdateObstacles();
            
            // Update material properties
            material.SetFloat("_ScaleY", scaleY);
            material.SetColor("_Color", obstacleColor); // Update color
            
            // Update quad position, size and scale
            UpdateQuadPosition();
            UpdateQuadSize();
            UpdateQuadScale();
        }
    }
    
    void UpdateQuadPosition()
    {
        // Move the entire quad using the offset
        transform.position = new Vector3(offsetX, offsetY, 0);
    }
    
    void UpdateQuadSize()
    {
        Vector2 size = GetQuadSize();
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        
        // Update the quad vertices to match the determined size
        Vector3[] vertices = {
            new Vector3(-halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, halfHeight, 0),
            new Vector3(-halfWidth, halfHeight, 0)
        };
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            meshFilter.mesh.vertices = vertices;
            meshFilter.mesh.RecalculateBounds();
        }
    }
    
    void UpdateObstacles()
    {
        if (simulation.obstacles.Count > 0)
        {
            // Convert obstacles to float4 array for the shader
            float4[] obstacleData = new float4[Mathf.Min(simulation.obstacles.Count, MAX_OBSTACLES)];
            
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
