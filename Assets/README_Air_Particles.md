# Simulación de Fluidos con Partículas de Aire

## Descripción

Esta simulación ahora incluye dos tipos de partículas:
- **Partículas de Fluido**: Comportamiento tradicional de fluido con gravedad
- **Partículas de Aire**: Partículas más ligeras y compresibles que interactúan con el fluido

## Características de las Partículas

### Partículas de Fluido (Tipo 0)
- **Gravedad**: -12.0 (caen hacia abajo)
- **Densidad objetivo**: 55
- **Presión**: 500
- **Viscosidad**: 0.06
- **Masa**: 1.0
- **Compresibilidad**: 1.0
- **Color**: Cyan (con gradiente de velocidad)
- **Spawn**: En la región central configurada

### Partículas de Aire (Tipo 1)
- **Gravedad**: 0.0 (sin gravedad)
- **Densidad objetivo**: 8 (mucho menos denso)
- **Presión**: 150 (menos presión)
- **Viscosidad**: 0.02 (menos viscoso)
- **Masa**: 0.2 (más ligero)
- **Compresibilidad**: 3.0 (más compresible)
- **Color**: Blanco sólido
- **Spawn**: En regiones marcadas como 's' en el CSV

## Interacción entre Tipos

Las partículas de fluido y aire interactúan entre sí con una fuerza reducida (configurable con `fluidAirInteractionStrength`). Esto permite:
- Transferencia de momento entre fluidos y aire
- El aire puede ser empujado por el fluido
- El fluido puede ser influenciado por el aire

## Configuración de Spawn

### Partículas de Fluido
- Se generan en la región central configurada en el `ParticleSpawner`
- Usan el algoritmo de spawn tradicional

### Partículas de Aire
- Se generan en regiones marcadas como 's' en el archivo CSV
- Usan spawn por cuadrícula 1x1 (una partícula por unidad cuadrada)
- Jitter mínimo para evitar alineación perfecta

## Archivo CSV

El archivo `obstacles_with_air.csv` contiene:
- **Obstáculos**: Marcados con 'o' (obstacle)
- **Regiones de spawn de aire**: Marcadas con 's' (spawn)

Formato:
```
class,pos_x,pos_y,width,height
o,-10.000,10.000,20.000,0.500  # Obstáculo
s,-8.000,8.000,4.000,4.000     # Región de spawn de aire
```

## Controles

- **Espacio**: Pausar/Reanudar simulación
- **R**: Resetear partículas
- **O**: Recargar obstáculos desde archivo
- **P**: Guardar obstáculos actuales

## Parámetros Ajustables

### Configuración de Partículas
- `fluidConfig`: Propiedades específicas del fluido
- `airConfig`: Propiedades específicas del aire
- `fluidAirInteractionStrength`: Fuerza de interacción entre tipos

### Configuración de Spawn
- `particleCount`: Número de partículas por tipo
- `spawnRegions`: Regiones de spawn de aire (cargadas desde CSV)

## Notas Técnicas

- Las partículas de aire no tienen gravedad, por lo que flotan libremente
- La interacción entre tipos permite transferencia de momento
- El aire es más compresible, permitiendo mayor variación de densidad
- Las partículas de aire son más pequeñas visualmente para distinguirlas 
