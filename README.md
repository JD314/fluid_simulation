# Fluid-Sim

Note: this branch shows the state of the project after [this video](https://youtu.be/rSKMYc1CQHE?si=KNw_i1sN2_CWEmzA).
<br>For the latest version of the project, see the [main branch](https://github.com/SebLague/Fluid-Sim/tree/main).

With thanks to the following papers:
* https://matthias-research.github.io/pages/publications/sca03.pdf
* https://web.archive.org/web/20250106201614/http://www.ligum.umontreal.ca/Clavet-2005-PVFS/pvfs.pdf
* https://sph-tutorial.physics-simulation.org/pdf/SPH_Tutorial.pdf
* https://web.archive.org/web/20140725014123/https://docs.nvidia.com/cuda/samples/5_Simulations/particles/doc/particles.pdf

![Fluid Sim](https://raw.githubusercontent.com/SebLague/Images/master/Fluid%20vid%20thumb.jpg)

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
