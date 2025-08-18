function binaryTreeMaze(width, height, startSide = 0, startCoord = 0.5, endSide = 3, endCoord = 0.5) {
    // Asegurar dimensiones impares
    width -= width % 2; width++;
    height -= height % 2; height++;

    // Inicializar laberinto lleno de muros (0 = muro, 1 = camino)
    const maze = [];
    for (let i = 0; i < height; i++) {
        maze.push([]);
        for (let j = 0; j < width; j++) {
            maze[i].push((i % 2 === 1 && j % 2 === 1) ? 1 : 0);
        }
    }

    // Algoritmo Binary Tree
    for (let x = 1; x < width; x += 2) {
        for (let y = 1; y < height; y += 2) {
            let south = Math.floor(Math.random() * 2);
            if (y === height - 2) south = 0;
            if (x === width - 2) south = 1;
            if (x === width - 2 && y === height - 2) break;

            if (south) maze[y + 1][x] = 1;
            else maze[y][x + 1] = 1;
        }
    }

    // Función para convertir coordenadas relativas en coordenadas absolutas válidas
    const computeCoord = (side, coord) => {
        let pos = Math.floor(coord * ((side % 2 === 0) ? width : height));
        if (pos % 2 === 0) pos += 1;
        pos = Math.max(1, Math.min((side % 2 === 0 ? width : height) - 2, pos));
        return pos;
    };

    // Apertura entrada
    const startX = [computeCoord(0, startCoord), 0, width - 1, computeCoord(3, startCoord)];
    const startY = [height - 1, computeCoord(1, startCoord), computeCoord(2, startCoord), 0];
    maze[startY[startSide]][startX[startSide]] = 1;

    // Apertura salida
    const endX = [computeCoord(0, endCoord), 0, width - 1, computeCoord(3, endCoord)];
    const endY = [height - 1, computeCoord(1, endCoord), computeCoord(2, endCoord), 0];
    maze[endY[endSide]][endX[endSide]] = 1;

    return maze;
}

// Permitir que se ejecute como script desde la terminal o se importe como módulo
if (require.main === module) {
    const [,, width, height, startSide, startCoord, endSide, endCoord] = process.argv;
    const result = binaryTreeMaze(
        parseInt(width),
        parseInt(height),
        parseInt(startSide),
        parseFloat(startCoord),
        parseInt(endSide),
        parseFloat(endCoord)
    );
    console.log(JSON.stringify(result)); // salida como matriz de 0 y 1
} else {
    module.exports = binaryTreeMaze;
}
