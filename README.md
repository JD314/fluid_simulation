# Fluid-Sim: Simulación de Fluidos 2D con Partículas de Aire

## 🎯 Descripción del Proyecto

**Fluid-Sim** es una simulación avanzada de fluidos 2D desarrollada en Unity que incluye un sistema completo de partículas de fluido y aire, con capacidades de optimización, laberintos personalizables y una interfaz de usuario intuitiva con flechas desplegables.

## ✨ Características Principales

### 🔴 **Sistema de Partículas Dual**
- **Partículas de Fluido**: Simulación física realista con presión, viscosidad y gravedad
- **Partículas de Aire**: Sistema de aire interactivo con optimización automática
- **Interacción Fluido-Aire**: Física realista entre ambos tipos de partículas

### 🚀 **Optimización Inteligente**
- **Limpieza Automática**: Eliminación automática de partículas de aire fuera de límites Y
- **Límites Configurables**: Parámetros `yMin` y `yMax` para definir la zona de aire activa
- **Visualización de Bounds**: Caja azul transparente que muestra los límites de optimización
- **Reactivación Inteligente**: Reutilización de partículas inactivas cuando es necesario

### 🏗️ **Sistema de Laberintos y Obstáculos**
- **Carga desde CSV**: Importación de laberintos desde archivos CSV
- **Obstáculos Dinámicos**: Sistema de obstáculos con colisiones realistas
- **Regiones de Spawn**: Zonas personalizables para generar partículas
- **Barreras Horizontales**: Sistema de barreras configurables

### 👁️ **Partículas de Aire Invisibles**
- **Toggle de Visibilidad**: Opción para hacer las partículas de aire completamente invisibles
- **Optimización Visual**: Reduce la carga visual sin afectar la simulación física
- **Control en Tiempo Real**: Cambio instantáneo de visibilidad

### 🎮 **Interfaz de Usuario Avanzada**
- **Flechas Desplegables**: Secciones colapsables con indicadores visuales (▼/▶)
- **Editores Personalizados**: Inspectores optimizados para cada tipo de script
- **Presets de Configuración**: Configuraciones predefinidas para diferentes escenarios
- **Botones de Acción Rápidos**: Controles directos para funcionalidades clave

## 🎯 **FUNCIONALIDAD DESTACADA: Carga de Laberintos CSV**

### **✅ Sistema Completo de Laberintos**
**Fluid-Sim incluye un sistema completo y funcional de carga de laberintos desde archivos CSV:**

- **📁 Carga Automática**: Los laberintos se cargan automáticamente al iniciar la simulación
- **🔄 Recarga en Tiempo Real**: Presiona **O** para recargar el laberinto sin reiniciar
- **💾 Guardado**: Presiona **P** para guardar obstáculos modificados
- **🎮 Controles Directos**: Botones en el inspector para todas las operaciones

### **📋 Formato CSV Soportado**
```csv
class,pos_x,pos_y,width,height
o,0.000,15.000,30.000,1.000    # Obstáculo (pared)
o,0.000,-15.000,30.000,1.000   # Obstáculo (pared)
s,10.000,10.000,4.000,4.000    # Región de spawn de aire
```

### **🔧 Configuración Rápida**
1. **Habilita "Usar Laberinto"** en el inspector de Simulation2D
2. **Configura "Archivo de Obstáculos"** (ej: `mazes_csv/mi_laberinto.csv`)
3. **Ejecuta la simulación** - ¡El laberinto se carga automáticamente!

### **🎯 Configuración Rápida con Selector de Archivos**
1. **Habilita "Usar Laberinto"** en el inspector de Simulation2D
2. **Haz clic en "📁 Seleccionar"** para abrir el selector de archivos nativo
3. **Navega y selecciona** tu archivo CSV desde la carpeta `mazes_csv/`
4. **El sistema automáticamente** convierte la ruta y carga el laberinto
5. **Ejecuta la simulación** - ¡El laberinto se carga automáticamente!

### **📁 Selector de Archivos Integrado**
**¡Nueva funcionalidad!** El inspector de Unity ahora incluye un **botón "📁 Seleccionar"** que:
- **Abre el selector de archivos** nativo del sistema operativo
- **Filtra solo archivos CSV** para facilitar la selección
- **Convierte automáticamente** las rutas absolutas a relativas
- **Sugiere la carpeta `mazes_csv/`** como ubicación por defecto
- **Actualiza el campo** "Archivo de Obstáculos" automáticamente

## 📁 Estructura del Proyecto

### **Scripts Principales**
```
Assets/Scripts/
├── Simulation2D.cs                    # Script principal de simulación
├── ObstacleLoader.cs                   # Cargador de laberintos desde CSV
├── Display/                            # Sistema de renderizado
│   ├── ParticleDisplayGPU.cs          # Renderizado de partículas
│   ├── ObstacleDisplay2D.cs           # Renderizado de obstáculos
│   ├── SimpleObstacleDisplay2D.cs     # Renderizado simplificado
│   └── Particle2D.shader              # Shader con soporte para invisibilidad
├── Compute/                            # Shaders de computación GPU
│   └── FluidSim2D.compute             # Lógica de simulación GPU
├── Compute Helpers/                    # Utilidades para GPU
├── Examples/                           # Scripts de ejemplo y pruebas
│   ├── AirParticleConfigExample.cs    # Presets y configuraciones
│   ├── AirParticlesVisibilityTest.cs  # Script de pruebas
│   └── AirParticleTest.cs             # Tests básicos
└── Editor/                             # Editores personalizados del inspector
    ├── Simulation2DEditor.cs          # Editor principal con flechas desplegables
    ├── AirParticleConfigExampleEditor.cs
    └── AirParticlesVisibilityTestEditor.cs
```

### **Archivos de Datos**
```
Assets/
├── Materials/                          # Materiales para renderizado
├── Scenes/                             # Escenas de Unity
├── mazes_csv/                          # Archivos CSV de laberintos
│   ├── maze_simple.csv                 # Laberinto básico
│   ├── maze_complex.csv                # Laberinto complejo
│   ├── obstacles_example.csv           # Ejemplo de obstáculos
│   ├── obstacles_with_air.csv          # Obstáculos con regiones de spawn
│   └── obstacles_test.csv              # Laberinto de prueba
└── Mazes_nb/                           # Imágenes de referencia de laberintos
```

### **Scripts de Generación**
```
Maze_generation/
├── maze_generation.py                  # Script principal de generación
├── scripts/                            # Scripts JavaScript para web
├── style/                              # Estilos CSS para interfaz web
└── output/                             # Archivos de salida generados
```

## 🚀 Cómo Usar

### **1. Configuración Inicial**
1. Abre el proyecto en Unity 2022.3.21f1 o superior
2. Abre la escena `Assets/Scenes/Maze.unity`
3. Selecciona el GameObject `Simulation2D` en la jerarquía

### **2. Configuración de Partículas**
- **Fluido**: Ajusta parámetros físicos, visuales y de spawn
- **Aire**: Configura comportamiento, visibilidad y optimización
- **Usa las flechas desplegables** (▼/▶) para organizar las secciones

### **3. Optimización de Aire**
- **Habilita "enableAirCleanup"** para limpieza automática
- **Configura "yMin" y "yMax"** para definir límites de optimización
- **Activa "showAirBounds"** para visualizar la zona de aire activa

### **4. Control de Visibilidad**
- **En la sección Visual del Aire**: Marca "Partículas de Aire Invisibles"
- **Las partículas se ocultan** instantáneamente sin afectar la simulación

### **5. Laberintos y Obstáculos**
- **Activa "useMaze"** para cargar laberintos desde CSV
- **Usa "📁 Seleccionar"** para elegir archivos CSV fácilmente
- **Configura "enableHorizontalBarriers"** para barreras automáticas

## ⚙️ Parámetros Clave

### **Simulación**
- `timeScale`: Velocidad de la simulación
- `iterationsPerFrame`: Calidad vs. rendimiento
- `smoothingRadius`: Radio de influencia de las partículas

### **Optimización de Aire**
- `enableAirCleanup`: Habilita limpieza automática
- `airCleanupFrequency`: Frecuencia de limpieza (en frames)
- `maxBatchSize`: Tamaño máximo de lote para limpieza

### **Interacción**
- `interactionRadius`: Radio de interacción con el mouse
- `interactionStrength`: Fuerza de interacción
- `fluidAirInteractionStrength`: Fuerza entre fluido y aire

## 🎨 Presets Disponibles

### **Configuraciones de Aire**
- **Aire Ligero**: Baja densidad, alta compresibilidad
- **Aire Medio**: Configuración balanceada
- **Aire Denso**: Alta densidad, baja compresibilidad
- **Aire Invisible**: Partículas ocultas para optimización visual
- **Aire Optimizado**: Con límites Y configurados

### **Configuraciones de Interacción**
- **Interacción Fuerte**: Alta fuerza de interacción
- **Interacción Media**: Fuerza balanceada
- **Interacción Débil**: Baja fuerza de interacción

## 🔧 Desarrollo y Extensión

### **Agregar Nuevos Tipos de Partículas**
1. Extiende la estructura `ParticleTypeConfig`
2. Agrega parámetros específicos
3. Implementa lógica en el compute shader
4. Crea editores personalizados si es necesario

### **Crear Nuevos Presets**
1. Modifica `AirParticleConfigExample.cs`
2. Agrega nuevos métodos de preset
3. Configura parámetros específicos
4. Actualiza la interfaz del editor

### **Optimización de Rendimiento**
- Ajusta `iterationsPerFrame` según tu hardware
- Usa `enableAirCleanup` para partículas de aire
- Configura límites Y apropiados para tu escena

## 📚 **DOCUMENTACIÓN COMPLETA DEL SISTEMA**

### **🔴 Sistema de Partículas de Aire**

#### **Física Realista del Aire**
- **Densidad Configurable**: Parámetro `targetDensity` para simular diferentes tipos de aire
- **Presión y Viscosidad**: Sistema de presión basado en SPH (Smoothed Particle Hydrodynamics)
- **Gravedad Personalizable**: Control independiente de la gravedad para el aire
- **Compresibilidad**: Parámetro `compressibility` para simular diferentes comportamientos

#### **Sistema de Optimización Inteligente**
- **Limpieza Automática**: Eliminación automática de partículas fuera de límites Y
- **Límites Configurables**: Parámetros `yMin` y `yMax` para definir la zona activa
- **Reactivación Inteligente**: Reutilización de partículas inactivas cuando es necesario
- **Procesamiento por Lotes**: Control del tamaño máximo de lote para optimización

#### **Control de Visibilidad Avanzado**
- **Toggle de Invisibilidad**: Opción para ocultar completamente las partículas de aire
- **Optimización Visual**: Reduce la carga visual sin afectar la simulación física
- **Cambio en Tiempo Real**: Modificación instantánea de la visibilidad
- **Integración con Shader**: El shader `Particle2D.shader` maneja la invisibilidad

#### **Presets de Aire Predefinidos**

**1. Aire Ligero**
```csharp
gravity = 0.0f;           // Sin gravedad
targetDensity = 8f;       // Baja densidad
pressureMultiplier = 150f; // Presión moderada
viscosityStrength = 0.02f; // Baja viscosidad
mass = 0.2f;              // Masa ligera
compressibility = 3.0f;   // Alta compresibilidad
particleScale = 0.6f;     // Escala pequeña
```

**2. Aire Medio**
```csharp
gravity = -9.81f;         // Gravedad estándar
targetDensity = 15f;      // Densidad media
pressureMultiplier = 200f; // Presión balanceada
viscosityStrength = 0.05f; // Viscosidad media
mass = 0.3f;              // Masa media
compressibility = 2.0f;   // Compresibilidad media
particleScale = 0.7f;     // Escala media
```

**3. Aire Denso**
```csharp
gravity = -9.81f;         // Gravedad estándar
targetDensity = 25f;      // Alta densidad
pressureMultiplier = 300f; // Alta presión
viscosityStrength = 0.08f; // Alta viscosidad
mass = 0.5f;              // Masa alta
compressibility = 1.0f;   // Baja compresibilidad
particleScale = 0.8f;     // Escala grande
```

**4. Aire Invisible**
```csharp
// Configuración estándar + invisibilidad
airParticlesInvisible = true;  // Partículas ocultas
particleScale = 0.0f;          // Escala 0 para invisibilidad
```

**5. Aire Optimizado**
```csharp
// Configuración estándar + límites Y
yMin = -100f;                  // Límite inferior
yMax = 100f;                   // Límite superior
showAirBounds = true;          // Mostrar límites
enableAirCleanup = true;       // Limpieza habilitada
```

### **🏗️ Sistema de Laberintos y Obstáculos**

#### **Sistema de Laberintos CSV**
- **Importación Automática**: Carga de laberintos desde archivos CSV
- **Formato Flexible**: Soporte para obstáculos y regiones de spawn
- **Carga en Tiempo Real**: Recarga de laberintos sin reiniciar la simulación
- **Validación de Datos**: Verificación automática de formatos y rangos

#### **Obstáculos Dinámicos**
- **Colisiones Realistas**: Sistema de colisión basado en SPH
- **Expansión de Colisión**: Offset configurable para mejor interacción
- **Visualización de Hitboxes**: Opción para mostrar áreas de colisión
- **Obstáculos Rectangulares**: Sistema flexible de formas geométricas

#### **Barreras Horizontales**
- **Generación Automática**: Creación automática de barreras en posiciones Y específicas
- **Configuración Flexible**: Espaciado, ancho y alto personalizables
- **Integración con Laberintos**: Funciona independientemente del sistema de laberintos
- **Debug Visual**: Visualización de barreras en el editor

#### **Formato de Archivos CSV**

**Estructura del CSV**
```csv
class,pos_x,pos_y,width,height
o,-10.000,10.000,20.000,0.500
s,-8.000,8.000,4.000,4.000
o,5.000,-5.000,2.000,15.000
```

**Tipos de Elementos**
- **`o` (obstacle)**: Obstáculo sólido que bloquea partículas
- **`s` (spawn)**: Región donde se generan partículas de aire

**Parámetros de Posición**
- **`pos_x`**: Posición X del centro del elemento
- **`pos_y`**: Posición Y del centro del elemento
- **`width`**: Ancho del elemento
- **`height`**: Alto del elemento

**Ejemplo de Laberinto Completo**
```csv
class,pos_x,pos_y,width,height
# Paredes exteriores
o,0.000,15.000,30.000,1.000
o,0.000,-15.000,30.000,1.000
o,-15.000,0.000,1.000,30.000
o,15.000,0.000,1.000,30.000

# Obstáculos internos
o,0.000,5.000,10.000,1.000
o,0.000,-5.000,10.000,1.000
o,5.000,0.000,1.000,10.000
o,-5.000,0.000,1.000,10.000

# Regiones de spawn de aire
s,10.000,10.000,4.000,4.000
s,-10.000,-10.000,4.000,4.000
s,0.000,0.000,2.000,2.000
```

#### **Configuración del Sistema**

**Parámetros de Laberinto**
```csharp
[Header("Maze")]
public bool displayObstacle = true;           // Mostrar obstáculos
public bool useMaze = false;                  // Usar sistema de laberintos
public string obstacleFilePath = "";          // Ruta al archivo CSV
public bool loadObstaclesFromFile = true;     // Cargar desde archivo
public float obstacleOverlapOffset = 0.1f;   // Offset de superposición
```

**Configuración de Barreras**
```csharp
[Header("Barreras Horizontales")]
public bool enableHorizontalBarriers = false; // Habilitar barreras
public float barrierStartY = -5f;             // Y de inicio
public float barrierSpacing = 5f;             // Espaciado entre barreras
public float barrierWidth = 20f;              // Ancho de barrera
public float barrierHeight = 0.5f;            // Alto de barrera
```

**Configuración de Visualización**
```csharp
[Header("Debug y Visualización")]
public bool displayObstacleHitbox = true;     // Mostrar hitboxes
public float obstacleEdgeThreshold = 0.1f;    // Umbral de borde
```

### **🐍 Sistema de Generación de Laberintos**

#### **Scripts de Python**
- **Generación Procedural**: Algoritmos avanzados para crear laberintos únicos
- **Configuración Flexible**: Parámetros ajustables para complejidad y densidad
- **Exportación Automática**: Generación directa de archivos CSV
- **Visualización**: Gráficos de los laberintos generados

#### **Interfaz Web Interactiva**
- **Diseño Visual**: Interfaz intuitiva para crear laberintos
- **Edición en Tiempo Real**: Modificación interactiva de obstáculos
- **Exportación Instantánea**: Generación automática de archivos CSV
- **Previsualización**: Vista previa del laberinto antes de exportar

#### **Algoritmos de Generación**

**1. Backtracking**
```python
def generate_backtracking_maze(width, height):
    """
    Genera laberinto usando algoritmo de backtracking
    - Crea un camino que se ramifica
    - Garantiza que todas las áreas sean accesibles
    - Ideal para laberintos de exploración
    """
```

**2. Generación Procedural**
```python
def generate_procedural_maze(width, height, complexity, density):
    """
    Genera laberinto usando ruido procedural
    - Usa Perlin noise para crear patrones naturales
    - Controla complejidad y densidad
    - Resultados únicos cada vez
    """
```

**3. Laberintos Temáticos**
```python
def generate_themed_maze(theme, width, height):
    """
    Genera laberintos con temas específicos
    - Laberintos de castillo con torres
    - Laberintos de cueva con pasillos irregulares
    - Laberintos de jardín con patrones orgánicos
    """
```

#### **Uso de Python**

**Requisitos**
```bash
pip install numpy matplotlib
```

**Uso Básico**
```python
from maze_generation import generate_maze

# Generar laberinto básico
maze = generate_maze(width=20, height=20)

# Generar laberinto complejo
maze = generate_maze(
    width=30, 
    height=30, 
    complexity=0.8, 
    density=0.8
)

# Exportar a CSV
export_to_csv(maze, "mi_laberinto.csv")
```

**Parámetros Configurables**
```python
def generate_maze(width, height, complexity=0.75, density=0.75, seed=None):
    """
    Genera un laberinto procedural
    
    Args:
        width (int): Ancho del laberinto
        height (int): Alto del laberinto
        complexity (float): Complejidad (0.0 a 1.0)
        density (float): Densidad de obstáculos (0.0 a 1.0)
        seed (int): Semilla para reproducibilidad
    
    Returns:
        numpy.ndarray: Matriz del laberinto
    """
```

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

### **Simulación de Contenedor**
- Crea paredes exteriores con obstáculos
- Define regiones de spawn para partículas
- Configura barreras horizontales para compartimentos

### **Laberinto Interactivo**
- Diseña laberintos complejos con pasillos
- Coloca obstáculos estratégicamente
- Usa regiones de spawn para efectos especiales

### **Simulación de Tuberías**
- Crea canales con obstáculos
- Define entradas y salidas con regiones de spawn
- Configura barreras para control de flujo

## 🐛 Solución de Problemas

### **Error de Compilación C# 9.0**
- El proyecto está optimizado para C# 9.0
- Usa constructores explícitos en lugar de inicializadores de campos
- Compatible con Unity 2022.3.21f1

### **Las Partículas No Se Limpian**
- Verifica que `enableAirCleanup` esté habilitado
- Asegúrate de que `yMin` y `yMax` estén configurados correctamente
- Revisa que `airCleanupFrequency` no sea muy alto

### **Los Obstáculos No Se Cargan**
- Verifica que `useMaze` esté habilitado
- Asegúrate de que la ruta del archivo CSV sea correcta
- Revisa que el archivo CSV esté en formato correcto
- Verifica que las coordenadas estén dentro del rango de la escena

### **Las Colisiones No Funcionan**
- Ajusta `obstacleOverlapOffset` para mejor interacción
- Verifica que `displayObstacle` esté habilitado
- Asegúrate de que los obstáculos tengan dimensiones apropiadas
- Revisa que `obstacleEdgeThreshold` esté configurado correctamente

### **Problemas de Rendimiento**
- Reduce `iterationsPerFrame` si es necesario
- Ajusta `airCleanupFrequency` según tu hardware
- Considera usar `airParticlesInvisible` para optimización visual
- Reduce el número de obstáculos si es necesario

## 🔮 Futuras Mejoras

### **Funcionalidades Planificadas**
- [ ] Sistema de partículas 3D
- [ ] Más tipos de fluidos (agua, aceite, etc.)
- [ ] Efectos visuales avanzados
- [ ] Exportación de simulaciones
- [ ] API para integración externa
- [ ] Sistema de viento direccional
- [ ] Efectos de temperatura en el aire
- [ ] Simulación de humedad y condensación
- [ ] Editor visual de laberintos integrado en Unity
- [ ] Sistema de obstáculos con formas personalizadas
- [ ] Obstáculos móviles y animados

### **Optimizaciones Técnicas**
- [ ] Multi-threading para limpieza de partículas
- [ ] GPU compute shaders para optimización
- [ ] Sistema de LOD (Level of Detail) para partículas
- [ ] Compresión de datos de partículas
- [ ] Spatial partitioning para obstáculos
- [ ] LOD (Level of Detail) para obstáculos complejos
- [ ] Culling automático de obstáculos fuera de pantalla
- [ ] Sistema de instancing para obstáculos repetitivos

## 🤝 Contribuciones

Para contribuir al proyecto:
1. Fork del repositorio
2. Crea una rama para tu feature
3. Implementa cambios siguiendo el estilo del código
4. Actualiza la documentación correspondiente
5. Envía un Pull Request

## 📄 Licencia

Este proyecto está bajo la licencia especificada en el archivo `LICENSE`.

## 🎯 Roadmap

- [ ] Sistema de partículas 3D
- [ ] Más tipos de fluidos (agua, aceite, etc.)
- [ ] Efectos visuales avanzados
- [ ] Exportación de simulaciones
- [ ] API para integración externa

---

**Fluid-Sim** - Simulación de fluidos avanzada con optimización inteligente, interfaz intuitiva y sistema completo de laberintos CSV.
