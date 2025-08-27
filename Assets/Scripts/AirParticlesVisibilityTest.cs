using UnityEngine;

/// <summary>
/// Script de prueba para verificar que la opción de partículas de aire invisibles funciona correctamente
/// </summary>
public class AirParticlesVisibilityTest : MonoBehaviour
{
    [Header("Referencias")]
    public Simulation2D simulation;
    
    [Header("Pruebas")]
    [SerializeField] private bool testOnStart = false;
    [SerializeField] private bool showDebugInfo = true;
    
    private bool lastAirVisibilityState = false;
    
    void Start()
    {
        if (simulation == null)
        {
            simulation = FindObjectOfType<Simulation2D>();
        }
        
        if (testOnStart)
        {
            TestAirParticlesVisibility();
        }
    }
    
    void Update()
    {
        // Verificar cambios en la visibilidad del aire
        if (simulation != null && simulation.airConfig.airParticlesInvisible != lastAirVisibilityState)
        {
            lastAirVisibilityState = simulation.airConfig.airParticlesInvisible;
            if (showDebugInfo)
            {
                string status = lastAirVisibilityState ? "INVISIBLE" : "VISIBLE";
                Debug.Log($"[TEST] Estado del aire cambiado a: {status}");
            }
        }
    }
    
    /// <summary>
    /// Prueba la funcionalidad de partículas de aire invisibles
    /// </summary>
    [ContextMenu("Probar Visibilidad del Aire")]
    public void TestAirParticlesVisibility()
    {
        if (simulation == null)
        {
            Debug.LogError("[TEST] No se encontró la simulación!");
            return;
        }
        
        Debug.Log("=== PRUEBA DE VISIBILIDAD DE PARTÍCULAS DE AIRE ===");
        
        // Estado inicial
        Debug.Log($"Estado inicial - Aire invisible: {simulation.airConfig.airParticlesInvisible}");
        Debug.Log($"Escala del fluido: {simulation.fluidConfig.particleScale}");
        Debug.Log($"Escala del aire: {simulation.airConfig.particleScale}");
        
        // Probar hacer el aire invisible
        Debug.Log("\n--- Haciendo aire invisible ---");
        simulation.MakeAirInvisible();
        Debug.Log($"Después de MakeAirInvisible - Aire invisible: {simulation.airConfig.airParticlesInvisible}");
        
        // Esperar un frame
        StartCoroutine(TestAfterDelay(0.1f, () => {
            Debug.Log("--- Verificando después de hacer invisible ---");
            Debug.Log($"Estado del aire: {simulation.airConfig.airParticlesInvisible}");
            
            // Probar hacer el aire visible
            Debug.Log("\n--- Haciendo aire visible ---");
            simulation.MakeAirVisible();
            Debug.Log($"Después de MakeAirVisible - Aire invisible: {simulation.airConfig.airParticlesInvisible}");
            
            // Esperar otro frame
            StartCoroutine(TestAfterDelay(0.1f, () => {
                Debug.Log("--- Verificando después de hacer visible ---");
                Debug.Log($"Estado final del aire: {simulation.airConfig.airParticlesInvisible}");
                Debug.Log("=== PRUEBA COMPLETADA ===");
            }));
        }));
    }
    
    /// <summary>
    /// Prueba rápida de alternancia
    /// </summary>
    [ContextMenu("Prueba Rápida de Alternancia")]
    public void QuickToggleTest()
    {
        if (simulation == null) return;
        
        Debug.Log("--- PRUEBA RÁPIDA DE ALTERNANCIA ---");
        Debug.Log($"Estado antes: {(simulation.airConfig.airParticlesInvisible ? "Invisible" : "Visible")}");
        
        simulation.ToggleAirVisibility();
        
        Debug.Log($"Estado después: {(simulation.airConfig.airParticlesInvisible ? "Invisible" : "Visible")}");
    }
    
    /// <summary>
    /// Prueba con presets
    /// </summary>
    [ContextMenu("Probar Preset de Aire Invisible")]
    public void TestInvisiblePreset()
    {
        if (simulation == null) return;
        
        var configExample = FindObjectOfType<AirParticleConfigExample>();
        if (configExample != null)
        {
            Debug.Log("--- APLICANDO PRESET DE AIRE INVISIBLE ---");
            configExample.ApplyInvisibleAir();
        }
        else
        {
            Debug.LogWarning("No se encontró AirParticleConfigExample para probar presets");
        }
    }
    
    /// <summary>
    /// Corrutina para esperar un delay
    /// </summary>
    private System.Collections.IEnumerator TestAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    
    /// <summary>
    /// Muestra información de debug en la consola
    /// </summary>
    [ContextMenu("Mostrar Info de Debug")]
    public void ShowDebugInfo()
    {
        if (simulation == null) return;
        
        Debug.Log("=== INFORMACIÓN DE DEBUG ===");
        Debug.Log($"Simulación activa: {simulation.isActiveAndEnabled}");
        Debug.Log($"Partículas de fluido: {simulation.numFluidParticles}");
        Debug.Log($"Partículas de aire: {simulation.numAirParticles}");
        Debug.Log($"Total de partículas: {simulation.numParticles}");
        Debug.Log($"Aire invisible: {simulation.airConfig.airParticlesInvisible}");
        Debug.Log($"Escala del fluido: {simulation.fluidConfig.particleScale}");
        Debug.Log($"Escala del aire: {simulation.airConfig.particleScale}");
        Debug.Log($"Interacción fluido-aire: {simulation.fluidAirInteractionStrength}");
        Debug.Log($"Límites del aire - Y Min: {simulation.airConfig.yMin}, Y Max: {simulation.airConfig.yMax}");
        Debug.Log($"Mostrar límites del aire: {simulation.airConfig.showAirBounds}");
    }
}
