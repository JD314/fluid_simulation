using UnityEngine;

/// <summary>
/// Ejemplo de configuración para partículas de aire
/// Este script muestra cómo configurar diferentes tipos de partículas
/// </summary>
public class AirParticleConfigExample : MonoBehaviour
{
    [Header("Referencias")]
    public Simulation2D simulation;
    
    [Header("Configuraciones Predefinidas")]
    [SerializeField] private bool applyConfigOnStart = false;
    
    [System.Serializable]
    public struct AirConfigPreset
    {
        public string name;
        public float gravity;
        public float targetDensity;
        public float pressureMultiplier;
        public float nearPressureMultiplier;
        public float viscosityStrength;
        public float mass;
        public float compressibility;
        public int particleCount;

        public float particleScale;
        [Tooltip("Si está marcado, las partículas de aire serán invisibles")]
        public bool airParticlesInvisible;
        
        [Header("Optimización")]
        public float yMin;
        public float yMax;
        public bool showAirBounds;
    }
    
    [Header("Presets de Aire")]
    public AirConfigPreset[] airPresets = new AirConfigPreset[]
    {
        new AirConfigPreset
        {
            name = "Aire Ligero",
            gravity = 0.0f,
            targetDensity = 5f,
            pressureMultiplier = 100f,
            nearPressureMultiplier = 4f,
            viscosityStrength = 0.01f,
            mass = 0.1f,
            compressibility = 4.0f,
            particleCount = 200,
            particleScale = 0.5f,
            airParticlesInvisible = false,
            yMin = -30f,
            yMax = 30f,
            showAirBounds = false
        },
        new AirConfigPreset
        {
            name = "Aire Medio",
            gravity = 0.0f,
            targetDensity = 8f,
            pressureMultiplier = 150f,
            nearPressureMultiplier = 6f,
            viscosityStrength = 0.02f,
            mass = 0.2f,
            compressibility = 3.0f,
            particleCount = 300,
            particleScale = 0.6f,
            airParticlesInvisible = false,
            yMin = -40f,
            yMax = 40f,
            showAirBounds = false
        },
        new AirConfigPreset
        {
            name = "Aire Denso",
            gravity = 0.0f,
            targetDensity = 12f,
            pressureMultiplier = 200f,
            nearPressureMultiplier = 8f,
            viscosityStrength = 0.03f,
            mass = 0.3f,
            compressibility = 2.0f,
            particleCount = 400,
            particleScale = 0.7f,
            airParticlesInvisible = false,
            yMin = -50f,
            yMax = 50f,
            showAirBounds = false
        },
        new AirConfigPreset
        {
            name = "Aire Invisible",
            gravity = 0.0f,
            targetDensity = 8f,
            pressureMultiplier = 150f,
            nearPressureMultiplier = 6f,
            viscosityStrength = 0.02f,
            mass = 0.2f,
            compressibility = 3.0f,
            particleCount = 300,
            particleScale = 0.6f,
            airParticlesInvisible = true,
            yMin = -40f,
            yMax = 40f,
            showAirBounds = true
        },
        new AirConfigPreset
        {
            name = "Aire Optimizado",
            gravity = 0.0f,
            targetDensity = 8f,
            pressureMultiplier = 150f,
            nearPressureMultiplier = 6f,
            viscosityStrength = 0.02f,
            mass = 0.2f,
            compressibility = 3.0f,
            particleCount = 300,
            particleScale = 0.6f,
            airParticlesInvisible = false,
            yMin = -25f,
            yMax = 25f,
            showAirBounds = true
        }
    };
    
    void Start()
    {
        if (applyConfigOnStart)
        {
            ApplyDefaultConfig();
        }
    }
    
    /// <summary>
    /// Aplica la configuración por defecto
    /// </summary>
    [ContextMenu("Aplicar Configuración por Defecto")]
    public void ApplyDefaultConfig()
    {
        if (simulation == null)
        {
            simulation = FindObjectOfType<Simulation2D>();
        }
        
        if (simulation != null)
        {
            // Aplicar preset de aire medio
            ApplyAirPreset(1);
            Debug.Log("Configuración por defecto aplicada");
        }
    }
    
    /// <summary>
    /// Aplica un preset de configuración de aire
    /// </summary>
    /// <param name="presetIndex">Índice del preset a aplicar</param>
    public void ApplyAirPreset(int presetIndex)
    {
        if (simulation == null || presetIndex < 0 || presetIndex >= airPresets.Length)
        {
            Debug.LogError("Preset inválido o simulación no encontrada");
            return;
        }
        
        var preset = airPresets[presetIndex];
        
        simulation.airConfig.gravity = preset.gravity;
        simulation.airConfig.targetDensity = preset.targetDensity;
        simulation.airConfig.pressureMultiplier = preset.pressureMultiplier;
        simulation.airConfig.nearPressureMultiplier = preset.nearPressureMultiplier;
        simulation.airConfig.viscosityStrength = preset.viscosityStrength;
        simulation.airConfig.mass = preset.mass;
        simulation.airConfig.compressibility = preset.compressibility;
        simulation.airConfig.particleCount = preset.particleCount;
        simulation.airConfig.particleScale = preset.particleScale;
        simulation.airConfig.airParticlesInvisible = preset.airParticlesInvisible;
        simulation.airConfig.yMin = preset.yMin;
        simulation.airConfig.yMax = preset.yMax;
        simulation.airConfig.showAirBounds = preset.showAirBounds;
        
        // Trigger update in simulation
        simulation.UpdateParticleDisplay();
        
        Debug.Log($"Preset '{preset.name}' aplicado al aire");
    }
    
    /// <summary>
    /// Aplica el preset de aire ligero
    /// </summary>
    [ContextMenu("Aplicar Aire Ligero")]
    public void ApplyLightAir()
    {
        ApplyAirPreset(0);
    }
    
    /// <summary>
    /// Aplica el preset de aire medio
    /// </summary>
    [ContextMenu("Aplicar Aire Medio")]
    public void ApplyMediumAir()
    {
        ApplyAirPreset(1);
    }
    
    /// <summary>
    /// Aplica el preset de aire denso
    /// </summary>
    [ContextMenu("Aplicar Aire Denso")]
    public void ApplyDenseAir()
    {
        ApplyAirPreset(2);
    }
    
    /// <summary>
    /// Aplica el preset de aire invisible
    /// </summary>
    [ContextMenu("Aplicar Aire Invisible")]
    public void ApplyInvisibleAir()
    {
        ApplyAirPreset(3);
    }
    
    /// <summary>
    /// Aplica el preset de aire optimizado
    /// </summary>
    [ContextMenu("Aplicar Aire Optimizado")]
    public void ApplyOptimizedAir()
    {
        ApplyAirPreset(4);
    }
    
    /// <summary>
    /// Configura una interacción fuerte entre fluido y aire
    /// </summary>
    [ContextMenu("Configurar Interacción Fuerte")]
    public void SetStrongInteraction()
    {
        if (simulation != null)
        {
            simulation.fluidAirInteractionStrength = 0.8f;
            Debug.Log("Interacción fluido-aire configurada como fuerte (0.8)");
        }
    }
    
    /// <summary>
    /// Configura una interacción débil entre fluido y aire
    /// </summary>
    [ContextMenu("Configurar Interacción Débil")]
    public void SetWeakInteraction()
    {
        if (simulation != null)
        {
            simulation.fluidAirInteractionStrength = 0.1f;
            Debug.Log("Interacción fluido-aire configurada como débil (0.1)");
        }
    }
    
    /// <summary>
    /// Configura una interacción media entre fluido y aire
    /// </summary>
    [ContextMenu("Configurar Interacción Media")]
    public void SetMediumInteraction()
    {
        if (simulation != null)
        {
            simulation.fluidAirInteractionStrength = 0.3f;
            Debug.Log("Interacción fluido-aire configurada como media (0.3)");
        }
    }
    
    /// <summary>
    /// Alterna entre aire visible e invisible
    /// </summary>
    [ContextMenu("Alternar Visibilidad del Aire")]
    public void ToggleAirVisibility()
    {
        if (simulation != null)
        {
            simulation.airConfig.airParticlesInvisible = !simulation.airConfig.airParticlesInvisible;
            string status = simulation.airConfig.airParticlesInvisible ? "invisible" : "visible";
            Debug.Log($"Aire configurado como {status}");
            // Trigger update in simulation
            simulation.UpdateParticleDisplay();
        }
    }
    
    /// <summary>
    /// Muestra información sobre la configuración actual
    /// </summary>
    [ContextMenu("Mostrar Configuración Actual")]
    public void ShowCurrentConfig()
    {
        if (simulation == null)
        {
            simulation = FindObjectOfType<Simulation2D>();
        }
        
        if (simulation != null)
        {
            Debug.Log("=== CONFIGURACIÓN ACTUAL ===");
            Debug.Log($"Fluido: {simulation.fluidConfig.particleCount} partículas, Gravedad: {simulation.fluidConfig.gravity}");
            Debug.Log($"Aire: {simulation.airConfig.particleCount} partículas, Gravedad: {simulation.airConfig.gravity}");
            Debug.Log($"Interacción fluido-aire: {simulation.fluidAirInteractionStrength}");
            Debug.Log($"Densidad fluido: {simulation.fluidConfig.targetDensity}, Aire: {simulation.airConfig.targetDensity}");
            Debug.Log($"Compresibilidad fluido: {simulation.fluidConfig.compressibility}, Aire: {simulation.airConfig.compressibility}");
        }
    }
}
