# Sistema de Partículas de Aire - Fluid-Sim

## 🎯 Descripción General

El sistema de partículas de aire en Fluid-Sim es una implementación avanzada que proporciona simulación física realista del aire, optimización inteligente de rendimiento y control completo de la visibilidad. Este sistema trabaja en conjunto con las partículas de fluido para crear interacciones físicas realistas.

## ✨ Características Principales

### 🔬 **Física Realista del Aire**
- **Densidad Configurable**: Parámetro `targetDensity` para simular diferentes tipos de aire
- **Presión y Viscosidad**: Sistema de presión basado en SPH (Smoothed Particle Hydrodynamics)
- **Gravedad Personalizable**: Control independiente de la gravedad para el aire
- **Compresibilidad**: Parámetro `compressibility` para simular diferentes comportamientos

### 🚀 **Sistema de Optimización Inteligente**
- **Limpieza Automática**: Eliminación automática de partículas fuera de límites Y
- **Límites Configurables**: Parámetros `yMin` y `yMax` para definir la zona activa
- **Reactivación Inteligente**: Reutilización de partículas inactivas cuando es necesario
- **Procesamiento por Lotes**: Control del tamaño máximo de lote para optimización

### 👁️ **Control de Visibilidad Avanzado**
- **Toggle de Invisibilidad**: Opción para ocultar completamente las partículas de aire
- **Optimización Visual**: Reduce la carga visual sin afectar la simulación física
- **Cambio en Tiempo Real**: Modificación instantánea de la visibilidad
- **Integración con Shader**: El shader `Particle2D.shader` maneja la invisibilidad

### 🎮 **Interfaz de Usuario Intuitiva**
- **Flechas Desplegables**: Secciones organizadas con indicadores visuales (▼/▶)
- **Editor Personalizado**: Inspector optimizado con secciones colapsables
- **Presets Predefinidos**: Configuraciones listas para usar
- **Control Directo**: Acceso rápido a funcionalidades clave

## 📁 Estructura de Archivos

### **Scripts Principales**
```
Assets/Scripts/
├── Simulation2D.cs                    # Script principal con configuración de aire
├── Display/
│   ├── ParticleDisplayGPU.cs          # Renderizado de partículas
│   └── Particle2D.shader             # Shader con soporte para invisibilidad
├── Examples/
│   ├── AirParticleConfigExample.cs   # Presets y configuraciones
│   ├── AirParticlesVisibilityTest.cs # Script de pruebas
│   └── AirParticleTest.cs            # Tests básicos
└── Editor/
    ├── Simulation2DEditor.cs          # Editor principal con flechas desplegables
    ├── AirParticleConfigExampleEditor.cs
    └── AirParticlesVisibilityTestEditor.cs
```

### **Shaders y Compute Shaders**
```
Assets/Scripts/
├── Compute/
│   └── FluidSim2D.compute            # Lógica de simulación GPU
└── Display/
    └── Particle2D.shader             # Renderizado con soporte de invisibilidad
```

## ⚙️ Configuración del Sistema

### **Estructura ParticleTypeConfig**
```csharp
[System.Serializable]
public struct ParticleTypeConfig
{
    [Header("Física")]
    public float gravity;              // Gravedad aplicada al aire
    public float targetDensity;        // Densidad objetivo del aire
    public float pressureMultiplier;   // Multiplicador de presión
    public float nearPressureMultiplier; // Presión de partículas cercanas
    public float viscosityStrength;    // Fuerza de viscosidad
    public float mass;                 // Masa de cada partícula
    public float compressibility;      // Factor de compresibilidad
    
    [Header("Visual")]
    public float particleScale;        // Escala de renderizado
    public bool airParticlesInvisible; // Toggle de invisibilidad
    
    [Header("Spawn")]
    public int particleCount;          // Número de partículas
    public Vector2 initialVelocity;    // Velocidad inicial
    public float jitterStrength;       // Fuerza de variación inicial
    
    [Header("Optimización (Solo Aire)")]
    public float yMin;                 // Límite mínimo en Y
    public float yMax;                 // Límite máximo en Y
    public bool showAirBounds;         // Mostrar límites de optimización
}
```

### **Parámetros de Optimización**
```csharp
[Header("Optimization Settings")]
public bool enableAirCleanup = true;           // Habilitar limpieza automática
public int airCleanupFrequency = 30;          // Frecuencia de limpieza (frames)
public int maxBatchSize = 50;                 // Tamaño máximo de lote
```

## 🎨 Presets Disponibles

### **Configuraciones de Aire Predefinidas**

#### **1. Aire Ligero**
```csharp
gravity = 0.0f;           // Sin gravedad
targetDensity = 8f;       // Baja densidad
pressureMultiplier = 150f; // Presión moderada
viscosityStrength = 0.02f; // Baja viscosidad
mass = 0.2f;              // Masa ligera
compressibility = 3.0f;   // Alta compresibilidad
particleScale = 0.6f;     // Escala pequeña
```

#### **2. Aire Medio**
```csharp
gravity = -9.81f;         // Gravedad estándar
targetDensity = 15f;      // Densidad media
pressureMultiplier = 200f; // Presión balanceada
viscosityStrength = 0.05f; // Viscosidad media
mass = 0.3f;              // Masa media
compressibility = 2.0f;   // Compresibilidad media
particleScale = 0.7f;     // Escala media
```

#### **3. Aire Denso**
```csharp
gravity = -9.81f;         // Gravedad estándar
targetDensity = 25f;      // Alta densidad
pressureMultiplier = 300f; // Alta presión
viscosityStrength = 0.08f; // Alta viscosidad
mass = 0.5f;              // Masa alta
compressibility = 1.0f;   // Baja compresibilidad
particleScale = 0.8f;     // Escala grande
```

#### **4. Aire Invisible**
```csharp
// Configuración estándar + invisibilidad
airParticlesInvisible = true;  // Partículas ocultas
particleScale = 0.0f;          // Escala 0 para invisibilidad
```

#### **5. Aire Optimizado**
```csharp
// Configuración estándar + límites Y
yMin = -100f;                  // Límite inferior
yMax = 100f;                   // Límite superior
showAirBounds = true;          // Mostrar límites
enableAirCleanup = true;       // Limpieza habilitada
```

### **Configuraciones de Interacción**
```csharp
// Interacción Fuerte
fluidAirInteractionStrength = 0.8f;

// Interacción Media
fluidAirInteractionStrength = 0.5f;

// Interacción Débil
fluidAirInteractionStrength = 0.2f;
```

## 🚀 Cómo Usar

### **1. Configuración Básica**
1. Selecciona el GameObject `Simulation2D` en la jerarquía
2. En el inspector, expande la sección "🔴 Tipos de Partículas"
3. Expande "Configuración de Aire"
4. Ajusta los parámetros según tus necesidades

### **2. Activación de Optimización**
1. Expande la sección "🚀 Configuración de Optimización"
2. Marca "Habilitar Limpieza de Aire"
3. Configura "Frecuencia de Limpieza" (recomendado: 30 frames)
4. Ajusta "Tamaño Máximo de Lote" según tu hardware

### **3. Configuración de Límites Y**
1. En "Configuración de Aire", expande "🚀 Optimización (Solo Aire)"
2. Configura "Y Mínimo" y "Y Máximo" para definir la zona activa
3. Activa "Mostrar Límites del Aire" para visualizar la zona
4. Ajusta los valores según el tamaño de tu escena

### **4. Control de Visibilidad**
1. En "Configuración de Aire", expande "👁️ Visual"
2. Marca "Partículas de Aire Invisibles" para ocultarlas
3. Las partículas se ocultan instantáneamente
4. La simulación física continúa funcionando normalmente

### **5. Uso de Presets**
1. Agrega el script `AirParticleConfigExample` a tu escena
2. Configura la referencia a `Simulation2D`
3. Usa los botones del inspector para aplicar presets
4. Personaliza los presets según tus necesidades

## 🔧 Funcionalidades Avanzadas

### **Sistema de Limpieza Automática**
```csharp
// El sistema automáticamente:
// 1. Identifica partículas fuera de los límites Y
// 2. Las marca como inactivas (type = -1)
// 3. Las mueve fuera de la pantalla
// 4. Actualiza el contador de partículas activas
// 5. Reactiva partículas cuando es necesario
```

### **Reactivación Inteligente**
```csharp
// Cuando el número de partículas activas cae por debajo del 30%:
if (numAirParticles < airConfig.particleCount * 0.3f)
{
    ReactivateAirParticles(); // Reactiva partículas inactivas
}
```

### **Integración con Shader**
```hlsl
// En Particle2D.shader:
if (particleType == -1) // Partícula inactiva
{
    // Mover a posición invisible
    o.position = float4(0, 0, 0, 0);
    return;
}

if (particleScale <= 0.0) // Partícula invisible
{
    // Mover a posición invisible
    o.position = float4(0, 0, 0, 0);
    return;
}
```

## 📊 Monitoreo y Debug

### **Información de Debug**
```csharp
// El sistema registra información detallada:
Debug.Log($"Limpieza segura completada. Partículas de aire activas: {numAirParticles}");
Debug.Log($"Se desactivaron demasiadas partículas de aire. Reactivando...");
```

### **Visualización de Límites**
- **Caja Azul Transparente**: Muestra la zona de aire activa
- **Bordes Sólidos**: Límites exactos de optimización
- **Líneas Horizontales**: Marcadores en Y mínimo y máximo

### **Contadores en Tiempo Real**
- **Partículas Totales**: Número total de partículas de aire
- **Partículas Activas**: Partículas dentro de los límites Y
- **Partículas Inactivas**: Partículas fuera de los límites (reutilizables)

## 🎯 Casos de Uso

### **Simulación de Viento**
- Usa "Aire Ligero" con alta compresibilidad
- Configura límites Y amplios
- Ajusta la viscosidad para simular turbulencia

### **Optimización de Rendimiento**
- Usa "Aire Optimizado" con límites Y apropiados
- Habilita limpieza automática
- Configura frecuencia de limpieza según tu hardware

### **Simulación de Presión Atmosférica**
- Usa "Aire Denso" con baja compresibilidad
- Ajusta el multiplicador de presión
- Configura la gravedad apropiadamente

### **Demostración Visual**
- Usa "Aire Invisible" para ocultar partículas
- Mantén la simulación física activa
- Ideal para presentaciones o videos

## 🐛 Solución de Problemas

### **Las Partículas No Se Limpian**
- Verifica que `enableAirCleanup` esté habilitado
- Asegúrate de que `yMin` y `yMax` estén configurados correctamente
- Revisa que `airCleanupFrequency` no sea muy alto

### **La Optimización No Funciona**
- Activa `showAirBounds` para visualizar los límites
- Verifica que los límites Y estén dentro del rango de tu escena
- Asegúrate de que `maxBatchSize` sea apropiado para tu hardware

### **Las Partículas No Se Reactivan**
- El sistema reactiva automáticamente cuando cae por debajo del 30%
- Verifica que haya partículas inactivas disponibles
- Revisa los logs de debug para información detallada

### **Problemas de Rendimiento**
- Reduce `iterationsPerFrame` si es necesario
- Ajusta `airCleanupFrequency` según tu hardware
- Considera usar `airParticlesInvisible` para optimización visual

## 🔮 Futuras Mejoras

### **Funcionalidades Planificadas**
- [ ] Sistema de viento direccional
- [ ] Efectos de temperatura en el aire
- [ ] Simulación de humedad y condensación
- [ ] Integración con sistemas de partículas de Unity
- [ ] Exportación de datos de simulación

### **Optimizaciones Técnicas**
- [ ] Multi-threading para limpieza de partículas
- [ ] GPU compute shaders para optimización
- [ ] Sistema de LOD (Level of Detail) para partículas
- [ ] Compresión de datos de partículas

---

**Sistema de Partículas de Aire** - Optimización inteligente y control completo de la simulación del aire en Fluid-Sim. 
