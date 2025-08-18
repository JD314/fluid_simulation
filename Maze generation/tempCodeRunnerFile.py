f __name__ == "__main__":
    # Algoritmo
    choice=2  
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
    
    # import matplotlib.pyplot as plt
    # metadata, matriz = read_maze("Mazes/output/maze_0.txt")
    # scale = 1000 / matriz.shape[0]

    # plt.imshow(zoom(matriz, zoom=scale, order=0), cmap="gray_r")
    # # plt.savefig("Mazes/output/maze_9.png")

