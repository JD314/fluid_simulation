# Sistema de Obstáculos para Simulación de Fluidos

## Descripción
Este sistema permite cargar obstáculos desde archivos CSV para la simulación de fluidos 2D. Los obstáculos se definen como rectángulos con posición (centro) y tamaño.

## Formato del Archivo CSV
El archivo debe tener el siguiente formato:
```csv
pos_x,pos_y,width,height
0,0,1,1
5,5,2,1
-3,2,1.5,0.8
```

### Columnas:
- **pos_x**: Posición X del centro del obstáculo
- **pos_y**: Posición Y del centro del obstáculo  
- **width**: Ancho del obstáculo
- **height**: Alto del obstáculo

## Configuración en Unity

### En el Inspector de Simulation2D:
1. **Obstacle File Settings**:
   - `obstacleFilePath`: Ruta del archivo CSV (ej: "obstacles.csv")
   - `loadObstaclesFromFile`: Habilitar carga desde archivo
   - `reloadObstaclesOnStart`: Recargar al iniciar

2. **Laberinto**:
   - `useLaberinto`: Habilitar/deshabilitar el sistema de laberinto
   - `obstacles`: Lista de obstáculos (se llena automáticamente desde el archivo, oculta en inspector)

## Controles en Tiempo de Ejecución
- **O**: Recargar obstáculos desde archivo
- **P**: Guardar obstáculos actuales a archivo
- **R**: Reset de la simulación
- **Espacio**: Pausar/Reanudar

## Archivos de Ejemplo
- `obstacles.csv`: Obstáculos básicos de prueba
- `obstacles_example.csv`: Ejemplo más completo con 8 obstáculos

## Cómo Usar

1. **Crear un archivo CSV** con tus obstáculos
2. **Colocar el archivo** en la carpeta `Assets/mazes_csv/`
3. **Configurar** `obstacleFilePath` en el inspector (ej: "mazes_csv/mi_laberinto.csv")
4. **Ejecutar** la simulación

**Nota**: El sistema automáticamente busca en la carpeta `mazes_csv/` si no especificas la ruta completa.

## Ejemplo de Archivo CSV
```csv
pos_x,pos_y,width,height
0,0,1,1
5,5,2,1
-3,2,1.5,0.8
2,-4,0.8,1.2
```

## Sistema de Visualización

### ObstacleDisplay2D
El sistema de visualización ha sido actualizado para soportar múltiples obstáculos:

1. **Configuración en ObstacleDisplay2D**:
   - `useLaberinto`: Habilitar el sistema de visualización de laberinto
   - `maxObstacles`: Máximo número de obstáculos que puede renderizar (por defecto: 100)

2. **Compatibilidad**:
   - El sistema automáticamente detecta si usar el nuevo o legacy sistema
   - Mantiene compatibilidad total con el sistema anterior

3. **Rendimiento**:
   - Usa ComputeBuffer para transferir datos de obstáculos al shader
   - Renderiza todos los obstáculos en una sola pasada del shader

## Archivos de Ejemplo (en `Assets/mazes_csv/`)
- `obstacles.csv`: Obstáculos básicos de prueba
- `obstacles_example.csv`: Ejemplo más completo con 8 obstáculos
- `obstacles_test.csv`: Archivo de prueba con 10 obstáculos para demostrar la visualización
- `maze_simple.csv`: Laberinto simple con paredes y obstáculos
- `maze_complex.csv`: Laberinto complejo con múltiples pasillos y obstáculos

## Notas
- Los obstáculos se cargan automáticamente al iniciar si `loadObstaclesFromFile` está habilitado
- Puedes modificar el archivo CSV y presionar **O** para recargar sin reiniciar
- El sistema mantiene compatibilidad con el sistema legacy de obstáculos
- La lista de obstáculos está oculta en el inspector para una interfaz más limpia
- La visualización se actualiza automáticamente cuando cambias entre sistemas
