# Generación de Laberintos - Fluid-Sim

## 🎯 Descripción General

El sistema de generación de laberintos en Fluid-Sim proporciona herramientas avanzadas para crear laberintos procedurales y personalizados. Incluye scripts de Python para generación automática, una interfaz web interactiva y exportación directa a formato CSV compatible con Unity.

## ✨ Características Principales

### 🐍 **Scripts de Python**
- **Generación Procedural**: Algoritmos avanzados para crear laberintos únicos
- **Configuración Flexible**: Parámetros ajustables para complejidad y densidad
- **Exportación Automática**: Generación directa de archivos CSV
- **Visualización**: Gráficos de los laberintos generados

### 🌐 **Interfaz Web Interactiva**
- **Diseño Visual**: Interfaz intuitiva para crear laberintos
- **Edición en Tiempo Real**: Modificación interactiva de obstáculos
- **Exportación Instantánea**: Generación automática de archivos CSV
- **Previsualización**: Vista previa del laberinto antes de exportar

### 🎨 **Sistema de Estilos**
- **CSS Personalizado**: Diseño moderno y responsive
- **JavaScript Avanzado**: Funcionalidades interactivas
- **Temas Visuales**: Múltiples estilos para diferentes preferencias

### 📊 **Formatos de Salida**
- **CSV Estándar**: Compatible con Unity y otros sistemas
- **Imágenes PNG**: Visualización de los laberintos generados
- **Datos Estructurados**: Información detallada de cada laberinto

## 📁 Estructura del Proyecto

### **Scripts de Python**
```
Maze_generation/
├── maze_generation.py                  # Script principal de generación
├── prueba_generacion.ipynb             # Notebook de Jupyter para pruebas
├── prueba_generacion_optimizada.ipynb  # Notebook optimizado
└── tempCodeRunnerFile.py               # Archivo temporal de ejecución
```

### **Interfaz Web**
```
Maze_generation/
├── scripts/                            # Scripts JavaScript
│   ├── maze_editor.js                  # Editor principal de laberintos
│   ├── maze_generator.js               # Generador procedural
│   ├── maze_exporter.js                # Exportador a CSV
│   └── maze_visualizer.js              # Visualizador 3D
├── style/                              # Estilos CSS
│   └── maze_style.css                  # Estilos principales
└── index.html                          # Página principal
```

### **Archivos de Salida**
```
Maze_generation/
├── output/                             # Archivos generados
│   ├── maze_0.png                      # Imagen del laberinto 0
│   ├── maze_1.png                      # Imagen del laberinto 1
│   ├── maze_2.png                      # Imagen del laberinto 2
│   ├── maze_3.png                      # Imagen del laberinto 3
│   └── maze_data.txt                   # Datos del laberinto
└── maze_*.png                          # Imágenes de referencia
```

## 🚀 Cómo Usar

### **1. Generación con Python**

#### **Requisitos**
```bash
pip install numpy matplotlib
```

#### **Uso Básico**
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

#### **Parámetros Configurables**
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

### **2. Interfaz Web**

#### **Acceso**
1. Abre `index.html` en tu navegador
2. Usa la interfaz visual para diseñar laberintos
3. Configura parámetros de generación
4. Exporta directamente a CSV

#### **Funcionalidades**
- **Editor Visual**: Crea obstáculos arrastrando y soltando
- **Generador Procedural**: Genera laberintos automáticamente
- **Previsualización**: Ve el resultado antes de exportar
- **Exportación**: Descarga el archivo CSV listo para Unity

### **3. Notebooks de Jupyter**

#### **prueba_generacion.ipynb**
- Pruebas básicas de generación
- Visualización de resultados
- Experimentación con parámetros

#### **prueba_generacion_optimizada.ipynb**
- Algoritmos optimizados
- Comparación de rendimiento
- Análisis de calidad

## 🔧 Funcionalidades Avanzadas

### **Algoritmos de Generación**

#### **1. Backtracking**
```python
def generate_backtracking_maze(width, height):
    """
    Genera laberinto usando algoritmo de backtracking
    - Crea un camino que se ramifica
    - Garantiza que todas las áreas sean accesibles
    - Ideal para laberintos de exploración
    """
```

#### **2. Generación Procedural**
```python
def generate_procedural_maze(width, height, complexity, density):
    """
    Genera laberinto usando ruido procedural
    - Usa Perlin noise para crear patrones naturales
    - Controla complejidad y densidad
    - Resultados únicos cada vez
    """
```

#### **3. Laberintos Temáticos**
```python
def generate_themed_maze(theme, width, height):
    """
    Genera laberintos con temas específicos
    - Laberintos de castillo con torres
    - Laberintos de cueva con pasillos irregulares
    - Laberintos de jardín con patrones orgánicos
    """
```

### **Sistema de Exportación**

#### **Formato CSV Estándar**
```csv
class,pos_x,pos_y,width,height
o,0.000,15.000,30.000,1.000
o,0.000,-15.000,30.000,1.000
o,-15.000,0.000,1.000,30.000
o,15.000,0.000,1.000,30.000
s,10.000,10.000,4.000,4.000
```

#### **Validación de Datos**
```python
def validate_maze_data(maze_data):
    """
    Valida que los datos del laberinto sean correctos
    - Verifica rangos de coordenadas
    - Comprueba superposiciones
    - Valida formatos de datos
    """
```

### **Visualización Avanzada**

#### **Gráficos 2D**
```python
def plot_maze_2d(maze, title="Laberinto Generado"):
    """
    Crea visualización 2D del laberinto
    - Obstáculos en rojo
    - Regiones de spawn en verde
    - Escala apropiada
    """
```

#### **Gráficos 3D**
```python
def plot_maze_3d(maze, elevation=0.5):
    """
    Crea visualización 3D del laberinto
    - Altura basada en tipo de elemento
    - Rotación interactiva
    - Exportación a formatos 3D
    """
```

## 🎨 Personalización y Temas

### **Estilos CSS Personalizables**
```css
/* Tema Clásico */
.maze-editor.classic {
    --primary-color: #2c3e50;
    --secondary-color: #3498db;
    --accent-color: #e74c3c;
}

/* Tema Moderno */
.maze-editor.modern {
    --primary-color: #34495e;
    --secondary-color: #9b59b6;
    --accent-color: #f39c12;
}

/* Tema Oscuro */
.maze-editor.dark {
    --primary-color: #1a1a1a;
    --secondary-color: #4a90e2;
    --accent-color: #50c878;
}
```

### **Configuraciones de JavaScript**
```javascript
// Configuración del editor
const editorConfig = {
    gridSize: 1.0,           // Tamaño de la cuadrícula
    snapToGrid: true,        // Ajustar a cuadrícula
    showCoordinates: true,   // Mostrar coordenadas
    autoSave: true,          // Guardado automático
    theme: 'classic'         // Tema visual
};
```

## 📊 Análisis y Métricas

### **Métricas de Calidad**
```python
def analyze_maze_quality(maze):
    """
    Analiza la calidad del laberinto generado
    
    Returns:
        dict: Métricas de calidad
            - connectivity: Conectividad del laberinto
            - complexity: Nivel de complejidad
            - density: Densidad de obstáculos
            - balance: Balance entre áreas
    """
```

### **Comparación de Algoritmos**
```python
def compare_algorithms(width, height, iterations=100):
    """
    Compara diferentes algoritmos de generación
    
    Args:
        width, height: Dimensiones del laberinto
        iterations: Número de iteraciones para comparación
    
    Returns:
        dict: Resultados de la comparación
    """
```

## 🐛 Solución de Problemas

### **Problemas Comunes**

#### **1. Generación Lenta**
```python
# Solución: Optimizar parámetros
maze = generate_maze(
    width=20,           # Reducir tamaño
    height=20,          # Reducir tamaño
    complexity=0.5,     # Reducir complejidad
    density=0.5         # Reducir densidad
)
```

#### **2. Laberintos Demasiado Simples**
```python
# Solución: Aumentar complejidad
maze = generate_maze(
    width=30,
    height=30,
    complexity=0.9,     # Aumentar complejidad
    density=0.8         # Aumentar densidad
)
```

#### **3. Errores de Exportación**
```python
# Solución: Validar datos antes de exportar
if validate_maze_data(maze_data):
    export_to_csv(maze_data, "laberinto.csv")
else:
    print("Error: Datos del laberinto inválidos")
```

### **Debug y Logging**
```python
import logging

# Configurar logging
logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)

def generate_maze_with_logging(width, height):
    logger.info(f"Generando laberinto {width}x{height}")
    # ... generación del laberinto
    logger.info("Laberinto generado exitosamente")
```

## 🔮 Futuras Mejoras

### **Funcionalidades Planificadas**
- [ ] **Editor 3D**: Interfaz tridimensional para diseño
- [ ] **IA Generativa**: Uso de redes neuronales para generación
- [ ] **Templates**: Plantillas predefinidas para diferentes tipos
- [ ] **Colaboración**: Edición colaborativa en tiempo real
- [ ] **Integración Unity**: Plugin directo para Unity

### **Optimizaciones Técnicas**
- [ ] **Multi-threading**: Generación paralela de laberintos
- [ ] **GPU Compute**: Uso de shaders para generación
- [ ] **Compresión**: Algoritmos de compresión para laberintos grandes
- [ ] **Caching**: Sistema de caché para laberintos frecuentes

### **Integración Avanzada**
- [ ] **API REST**: Servicio web para generación remota
- [ ] **Base de Datos**: Almacenamiento de laberintos populares
- [ ] **Machine Learning**: Aprendizaje de preferencias del usuario
- [ ] **Exportación 3D**: Formatos para impresión 3D y VR

## 📚 Recursos Adicionales

### **Documentación Técnica**
- **[README Principal](../README.md)**: Visión general del proyecto
- **[README de Obstáculos](../Assets/README_Obstacles.md)**: Sistema de obstáculos
- **[README de Partículas de Aire](../Assets/README_Air_Particles.md)**: Sistema de aire

### **Ejemplos y Demos**
- **maze_simple.csv**: Laberinto básico para empezar
- **maze_complex.csv**: Laberinto avanzado para pruebas
- **obstacles_with_air.csv**: Ejemplo completo con spawn de aire

### **Herramientas de Desarrollo**
- **Python Scripts**: Generación procedural y análisis
- **Web Interface**: Diseño visual interactivo
- **Jupyter Notebooks**: Experimentación y pruebas
- **Unity Integration**: Exportación directa a CSV

---

**Sistema de Generación de Laberintos** - Herramientas avanzadas para crear entornos complejos y únicos en Fluid-Sim.


