import subprocess
import os
import numpy as np
import ast
from scipy.ndimage import zoom

def generate_maze(choice, width, height, start_side, start_rel, end_side, end_rel):
    """
    Genera un laberinto utilizando algoritmos de JavaScript.

    Esta función permite seleccionar un algoritmo de generación de laberintos,
    especificar sus dimensiones y definir las coordenadas relativas de entrada y salida.

    Parámetros
    ----------
    choice : int
        Algoritmo seleccionado (0, 1, 2, ...)
            [0] aldous-broder
            [1] backtracking
            [2] binary-tree
            [3] prims

    width : int
        Ancho del laberinto en celdas
    height : int
        Alto del laberinto en celdas
    start_side : int
        Lado de entrada (0: abajo, 1: derecha, 2: izquierda, 3: arriba)
    start_rel : float
        Posición relativa en el lado de entrada (entre 0 y 1)
    end_side : int
        Lado de salida (0: abajo, 1: derecha, 2: izquierda, 3: arriba)
    end_rel : float
        Posición relativa en el lado de salida (entre 0 y 1)

    Archivos generados
    ------------------
    Ubicación: Mazes/output/maze_{n}.txt
    Formato: Metadatos (algoritmo, dimensiones, entrada/salida) + matriz

    Requisitos
    ----------
    - Node.js instalado
    - Scripts JS ubicados en Mazes/scripts/
    - Carpeta Mazes/output/ existente

    Excepciones
    -----------
    - FileNotFoundError: Node.js no está instalado
    - Exception: Otros errores de ejecución
    """

    scripts_dir = "C:/Users/JD/Desktop/fluid_sim/Maze_generation/scripts"
    output_dir = "C:/Users/JD/Desktop/fluid_sim/Maze_generation/output"

    
    mazes = len([f for f in os.listdir(output_dir) if f.endswith(".txt")])


    scripts = [f for f in os.listdir(scripts_dir) if f.endswith(".js")]
    script_path = os.path.join(scripts_dir, scripts[choice])
    
    try:
        # Ejecuta el script con todos los argumentos necesarios
        result = subprocess.run(
            [
                "node", script_path,
                str(width), str(height),
                str(start_side), str(start_rel),
                str(end_side), str(end_rel)
            ],
            capture_output=True,
            text=True
        )

        if result.returncode != 0:
            raise Exception(f"Error ejecutando script JS:\n{result.stderr}")

        maze = result.stdout
        metadata = (
            f"algorithm: {scripts[choice][:-3]}\n"
            f"width: {width}\n"
            f"height: {height}\n"
            f"start: side {start_side}, relative {start_rel}\n"
            f"end: side {end_side}, relative {end_rel}\n"
        )

        with open(f"{output_dir}/maze_{mazes}.txt", "w", encoding="utf-8") as f:
            f.write(metadata)
            f.write(maze)

    except FileNotFoundError:
        raise FileNotFoundError("Node.js no está instalado o no se encuentra en el PATH.")
    except Exception as e:
        raise RuntimeError(f"Ocurrió un error al generar el laberinto: {e}")
    print("Laberinto generado correctamente")

def read_maze(maze_file):
    """
    Lee un archivo de laberinto y devuelve su metadata y matriz

    Args:
        maze_file (str): Ruta al archivo del laberinto

    Returns:
        tuple: (metadata, matriz) donde:
            - metadata (str): Información sobre el algoritmo y dimensiones
            - matriz (np.array): Matriz binaria representando el laberinto

    Raises:
        Exception: Si hay error al leer el archivo
    """
    try:
        with open(f"{maze_file}", "r") as f:
            lineas = f.readlines()

        metadata = ''.join(lineas[:5])  # metadata
        contenido = ''.join(lineas[5:]) # Ignora la metadata para obtener la matriz
        matriz = np.array(ast.literal_eval(contenido), dtype=int)

        return metadata, matriz
    except Exception as e:
        raise Exception("Error al leer el laberinto")


if __name__ == "__main__":
    # Algoritmo
    choice=0
    
    # Ancho y alto del laberinto
    width=21        
    height=21   
    # Entrada y posicion relativa [0: abajo, 1: derecha, 2: izquierda, 3: arriba]   
    start_side=3
    start_rel=0.5 
    # Salida y posicion relativa
    
    end_side=0
    end_rel=0.25      

    generate_maze(
    choice=choice,        
    width=width,        
    height=height,       
    start_side=start_side,     
    start_rel=start_rel,       
    end_side=end_side,      
    end_rel=end_rel
    )
    # print([f for f in os.listdir(scripts_dir) if f.endswith(".js")])

    # import matplotlib.pyplot as plt
    # metadata, matriz = read_maze("Mazes/output/maze_0.txt")
    # scale = 1000 / matriz.shape[0]

    # plt.imshow(zoom(matriz, zoom=scale, order=0), cmap="gray_r")
    # # plt.savefig("Mazes/output/maze_9.png")


