using UnityEngine;

public class ObstacleDisplay2D : MonoBehaviour
{
    public Shader obstacleShader;
    public Color obstacleColor = Color.white;
    public float worldScale = 20.0f;
    public float scaleY = 1.0f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;
    
    private Material material;
    private MeshRenderer meshRenderer;
    private Simulation2D simulation;
    
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
            meshRenderer.enabled = simulation.displayObstacle;
            
            if (simulation.displayObstacle)
            {
                // Update obstacle data and scale
                material.SetVector("_ObstacleSize", simulation.obstacleSize);
                material.SetVector("_ObstacleCentre", simulation.obstacleCentre);
                material.SetFloat("_Scale", worldScale);
                material.SetFloat("_ScaleY", scaleY);
                material.SetFloat("_OffsetX", offsetX);
                material.SetFloat("_OffsetY", offsetY);
            }
        }
    }
    
    void OnDestroy()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }
}
