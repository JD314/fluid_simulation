using UnityEngine;

/// <summary>
/// Script de prueba para verificar la funcionalidad de las partículas de aire
/// </summary>
public class AirParticleTest : MonoBehaviour
{
    [Header("Referencias")]
    public Simulation2D simulation;
    
    [Header("Configuración de Prueba")]
    public bool autoTestOnStart = true;
    public float testDelay = 2.0f;
    
    void Start()
    {
        if (autoTestOnStart)
        {
            Invoke("RunAirParticleTest", testDelay);
        }
    }
    
    /// <summary>
    /// Ejecuta una prueba completa de las partículas de aire
    /// </summary>
    [ContextMenu("Ejecutar Prueba de Partículas de Aire")]
    public void RunAirParticleTest()
    {
        if (simulation == null)
        {
            simulation = FindObjectOfType<Simulation2D>();
        }
        
        if (simulation == null)
        {
            Debug.LogError("No se encontró Simulation2D en la escena");
            return;
        }
        
        Debug.Log("=== PRUEBA DE PARTÍCULAS DE AIRE ===");
        
        // Verificar configuración
        TestConfiguration();
        
        // Verificar spawn de partículas
        TestParticleSpawn();
        
        // Verificar interacción
        TestInteraction();
        
        Debug.Log("=== PRUEBA COMPLETADA ===");
    }
    
    void TestConfiguration()
    {
        Debug.Log("Verificando configuración...");
        
        // Verificar configuraciones de partículas
        if (simulation.fluidConfig.particleCount > 0)
        {
            Debug.Log($"✓ Partículas de fluido configuradas: {simulation.fluidConfig.particleCount}");
        }
        else
        {
            Debug.LogWarning("✗ No hay partículas de fluido configuradas");
        }
        
        if (simulation.airConfig.particleCount > 0)
        {
            Debug.Log($"✓ Partículas de aire configuradas: {simulation.airConfig.particleCount}");
        }
        else
        {
            Debug.LogWarning("✗ No hay partículas de aire configuradas");
        }
        
        // Verificar propiedades específicas del aire
        if (simulation.airConfig.gravity == 0.0f)
        {
            Debug.Log("✓ Gravedad del aire configurada correctamente (0.0)");
        }
        else
        {
            Debug.LogWarning($"✗ Gravedad del aire incorrecta: {simulation.airConfig.gravity}");
        }
        
        if (simulation.airConfig.targetDensity < simulation.fluidConfig.targetDensity)
        {
            Debug.Log("✓ Densidad del aire menor que la del fluido");
        }
        else
        {
            Debug.LogWarning("✗ Densidad del aire no es menor que la del fluido");
        }
        
        if (simulation.airConfig.compressibility > simulation.fluidConfig.compressibility)
        {
            Debug.Log("✓ Aire más compresible que el fluido");
        }
        else
        {
            Debug.LogWarning("✗ Aire no es más compresible que el fluido");
        }
    }
    
    void TestParticleSpawn()
    {
        Debug.Log("Verificando spawn de partículas...");
        
        // Verificar número total de partículas
        int expectedTotal = simulation.numFluidParticles + simulation.numAirParticles;
        if (simulation.numParticles == expectedTotal)
        {
            Debug.Log($"✓ Número total de partículas correcto: {simulation.numParticles}");
        }
        else
        {
            Debug.LogWarning($"✗ Número total de partículas incorrecto: {simulation.numParticles} (esperado: {expectedTotal})");
        }
        
        // Verificar regiones de spawn
        if (simulation.spawnRegions.Count > 0)
        {
            Debug.Log($"✓ Regiones de spawn de aire encontradas: {simulation.spawnRegions.Count}");
            for (int i = 0; i < simulation.spawnRegions.Count; i++)
            {
                var region = simulation.spawnRegions[i];
                Debug.Log($"  - Región {i}: Pos({region.position.x:F1}, {region.position.y:F1}), Tamaño({region.size.x:F1}, {region.size.y:F1})");
            }
        }
        else
        {
            Debug.LogWarning("✗ No se encontraron regiones de spawn de aire");
        }
    }
    
    void TestInteraction()
    {
        Debug.Log("Verificando configuración de interacción...");
        
        if (simulation.fluidAirInteractionStrength > 0.0f && simulation.fluidAirInteractionStrength <= 1.0f)
        {
            Debug.Log($"✓ Fuerza de interacción fluido-aire configurada: {simulation.fluidAirInteractionStrength}");
        }
        else
        {
            Debug.LogWarning($"✗ Fuerza de interacción fluido-aire fuera de rango: {simulation.fluidAirInteractionStrength}");
        }
    }
    
    /// <summary>
    /// Verifica el estado actual de la simulación
    /// </summary>
    [ContextMenu("Verificar Estado Actual")]
    public void CheckCurrentState()
    {
        if (simulation == null)
        {
            simulation = FindObjectOfType<Simulation2D>();
        }
        
        if (simulation != null)
        {
            Debug.Log($"Estado actual:");
            Debug.Log($"- Simulación pausada: {simulation.isPaused}");
            Debug.Log($"- Partículas totales: {simulation.numParticles}");
            Debug.Log($"- Partículas de fluido: {simulation.numFluidParticles}");
            Debug.Log($"- Partículas de aire: {simulation.numAirParticles}");
            Debug.Log($"- Obstáculos: {simulation.obstacles.Count}");
            Debug.Log($"- Regiones de spawn: {simulation.spawnRegions.Count}");
        }
    }
    
    /// <summary>
    /// Regenera las partículas
    /// </summary>
    [ContextMenu("Regenerar Partículas")]
    public void RegenerateParticles()
    {
        if (simulation != null)
        {
            simulation.RegenerateParticles();
            Debug.Log("Partículas regeneradas");
        }
    }
    
    /// <summary>
    /// Recarga el laberinto
    /// </summary>
    [ContextMenu("Recargar Laberinto")]
    public void ReloadMaze()
    {
        if (simulation != null)
        {
            simulation.ReloadMaze();
            Debug.Log("Laberinto recargado");
        }
    }
}
