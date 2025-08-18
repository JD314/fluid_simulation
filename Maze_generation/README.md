## Maze generation — Cómo funciona y cómo usarlo

### Objetivo
Generar laberintos con distintos algoritmos en JS, revisarlos en Python y exportarlos a formatos que usa Unity:
- PNG transparente para vista previa
- CSV con obstáculos/spawns leído por `Assets/Scripts/ObstacleLoader.cs`

### Estructura de carpetas
- `Maze generation/scripts/`: algoritmos JS (`aldous-broder.js`, `backtracking.js`, `binary-tree.js`, `prims.js`).
- `Maze generation/output/`: se guardan los `.txt` con metadata + matriz del laberinto.
- `Maze generation/maze_generation.py`: wrapper en Python que ejecuta los scripts de Node y guarda el `.txt`.
- `Maze generation/prueba_generacion.ipynb`: notebook para visualizar y exportar a PNG/CSV.
- `Assets/Mazes_nb/`: PNGs generados para revisar.
- `Assets/Mazes_csv/`: CSV finales que carga Unity.

### Requisitos
- Node.js instalado y disponible en PATH.
- Python 3.x con paquetes: `numpy`, `scipy`, `matplotlib`, `Pillow`, `opencv-python`.
  - Instalación rápida: `pip install numpy scipy matplotlib Pillow opencv-python`

### 1) Generar un laberinto (desde Python)
Edita y ejecuta `Maze generation/maze_generation.py` (sección `if __name__ == "__main__":`). Parámetros principales:
- `choice`: algoritmo (0: aldous-broder, 1: backtracking, 2: binary-tree, 3: prims)
- `width`, `height`: dimensiones en celdas (se fuerzan a impares)
- `start_side`, `end_side`: lado de entrada/salida (0: abajo, 1: derecha, 2: izquierda, 3: arriba)
- `start_rel`, `end_rel`: posición relativa en el lado (0–1)

El script ejecuta Node y crea `Maze generation/output/maze_N.txt` con:
```
algorithm: <nombre>
width: <w>
height: <h>
start: side <s>, relative <r>
end: side <s>, relative <r>
[
  [0/1, 0/1, ...],
  ...
]
```
Si cambiaste nombres de carpetas, ajusta en `maze_generation.py` las rutas `scripts_dir` y `output_dir` para que apunten a `Maze generation/scripts` y `Maze generation/output`.

### 2) Previsualizar y guardar PNG
Abre `prueba_generacion.ipynb` y ejecuta la celda "Guardar el laberinto en un archivo png":
- Lee el `.txt` con `mg.read_maze("./output/maze_X.txt")`.
- Guarda un PNG en `Assets/Mazes_nb/` y lo vuelve transparente (el blanco se hace alfa 0).

### 3) Exportar a CSV para Unity
En el mismo notebook, ejecuta la celda "Reescalar la matriz... y exportar CSV":
- Reescala la matriz (por ejemplo a 42x42) con `cv2.INTER_NEAREST`.
- Opcional: "conecta" muros contiguos para evitar huecos.
- Centra coordenadas en (0,0) e invierte Y para que coincida con el sistema de Unity.
- Controla el tamaño final con `escala_x` y `escala_y` (si en Unity se ve enorme, usa valores pequeños p. ej. 0.5, 0.1, etc.).
- Genera `Assets/Mazes_csv/maze_X.csv` con encabezado:
```
class,pos_x,pos_y,width,height
```
  - `class = o` → obstáculo (muro)
  - `class = s` → spawn o zona libre

Unity carga este archivo con `ObstacleLoader.LoadMazeFromCSV(...)`, que crea `ObstacleRectangle` y `SpawnRegion` a partir de esas filas.

### Notas importantes
- Convención 0/1:
  - `aldous-broder`, `backtracking`, `prims`: 0 = camino, 1 = muro.
  - `binary-tree`: 1 = camino, 0 = muro.
  - El notebook asume 1 = muro. Si usas `binary-tree`, invierte la matriz antes de exportar (por ejemplo `matriz = 1 - matriz`).
- Dimensiones impares: los scripts fuerzan `width` y `height` a impares para celdas válidas.
- Aperturas: las entradas/salidas se colocan en celdas impares y se abren explícitamente en el borde indicado.
- Nombres de archivo: en Unity selecciona el CSV que quieras (`Assets/mazes_csv/*.csv`).

### ⚠️ Problema de rendimiento y solución
**Problema**: El notebook original genera un obstáculo individual por cada celda del laberinto (42x42 = 1764 obstáculos), lo cual:
- Excede el límite de 100 obstáculos en los displays de Unity
- Causa problemas de rendimiento
- Solo muestra una pequeña parte del laberinto

**Soluciones**:
1. **Aumenté el límite** en `ObstacleDisplay2D.cs` y `SimpleObstacleDisplay2D.cs` de 100 a 2000 obstáculos
2. **Creé `prueba_generacion_optimizada.ipynb`** que genera obstáculos más grandes y eficientes:
   - Encuentra rectángulos continuos de obstáculos
   - Reduce de ~1764 obstáculos a ~50-100 obstáculos
   - Mantiene la misma apariencia visual
   - Mejor rendimiento en Unity

**Recomendación**: Usa `prueba_generacion_optimizada.ipynb` en lugar del notebook original para generar CSVs más eficientes.

### Ejemplo rápido
1. Genera un `.txt` con Prim (21x21, entrada arriba 0.5, salida abajo 0.25):
   - Edita `maze_generation.py` con esos parámetros y ejecútalo con Python.
2. En el notebook, establece `maze = "maze_2"` (o el que generaste) y ejecuta:
   - Celda PNG → guarda en `Assets/Mazes_nb/`.
   - Celda CSV → guarda en `Assets/Mazes_csv/`.
3. En Unity, asigna ese CSV al sistema de carga; verás los cuadrados verdes (obstáculos) en la escena.


