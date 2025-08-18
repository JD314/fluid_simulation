function primsMaze(width, height, entrySide, entryRel, exitSide, exitRel) {
    // Asegurar impares
    width -= width % 2; width++;
    height -= height % 2; height++;

    // Crear laberinto lleno de muros
    const maze = Array.from({ length: height }, () => Array(width).fill(1));

    // Crear celda inicial aleatoria impar
    let start = [];
    do start[0] = Math.floor(Math.random() * height); while (start[0] % 2 === 0);
    do start[1] = Math.floor(Math.random() * width); while (start[1] % 2 === 0);
    maze[start[0]][start[1]] = 0;

    const openCells = [start];

    // Algoritmo de Prim modificado
    while (openCells.length > 0) {
        let index = Math.floor(Math.random() * openCells.length);
        let cell = openCells[index];
        let n = neighbors(maze, cell[0], cell[1]);

        while (n.length === 0) {
            openCells.splice(index, 1);
            if (openCells.length === 0) break;
            index = Math.floor(Math.random() * openCells.length);
            cell = openCells[index];
            n = neighbors(maze, cell[0], cell[1]);
        }
        if (openCells.length === 0) break;

        const choice = n[Math.floor(Math.random() * n.length)];
        openCells.push(choice);

        if (n.length === 1) openCells.splice(index, 1);

        maze[choice[0]][choice[1]] = 0;
        maze[(choice[0] + cell[0]) / 2][(choice[1] + cell[1]) / 2] = 0;
    }

    // Función para traducir coordenadas relativas a posiciones reales
    function getOpeningCoords(side, rel) {
        let i, j;
        const w = maze[0].length;
        const h = maze.length;
        switch (side) {
            case 0: // inferior
                i = h - 1;
                j = 1 + 2 * Math.floor(((w - 2) / 2) * rel);
                break;
            case 1: // derecha
                j = w - 1;
                i = 1 + 2 * Math.floor(((h - 2) / 2) * rel);
                break;
            case 2: // izquierda
                j = 0;
                i = 1 + 2 * Math.floor(((h - 2) / 2) * rel);
                break;
            case 3: // superior
                i = 0;
                j = 1 + 2 * Math.floor(((w - 2) / 2) * rel);
                break;
            default:
                i = 0; j = 1;
        }
        return [i, j];
    }

    // Aplicar entrada y salida
    const [ei, ej] = getOpeningCoords(entrySide, entryRel);
    const [si, sj] = getOpeningCoords(exitSide, exitRel);
    maze[ei][ej] = 0;

    // También abrir una celda vecina para asegurar conexión
    if (entrySide === 0) maze[ei - 1][ej] = 0;
    if (entrySide === 1) maze[ei][ej - 1] = 0;
    if (entrySide === 2) maze[ei][ej + 1] = 0;
    if (entrySide === 3) maze[ei + 1][ej] = 0;

    maze[si][sj] = 0;
    if (exitSide === 0) maze[si - 1][sj] = 0;
    if (exitSide === 1) maze[si][sj - 1] = 0;
    if (exitSide === 2) maze[si][sj + 1] = 0;
    if (exitSide === 3) maze[si + 1][sj] = 0;

    return maze;
}

function neighbors(maze, i, j) {
    const dirs = [[-2, 0], [2, 0], [0, -2], [0, 2]];
    const res = [];

    for (const [di, dj] of dirs) {
        const ni = i + di, nj = j + dj;
        if (ni > 0 && ni < maze.length && nj > 0 && nj < maze[0].length) {
            if (maze[ni][nj] === 1) res.push([ni, nj]);
        }
    }
    return res;
}

if (require.main === module) {
    const [,, width, height, entrySide, entryRel, exitSide, exitRel] = process.argv;
    const maze = primsMaze(
        parseInt(width),
        parseInt(height),
        parseInt(entrySide),
        parseFloat(entryRel),
        parseInt(exitSide),
        parseFloat(exitRel)
    );
    console.log(JSON.stringify(maze)); // 0 = camino, 1 = muro
} else {
    module.exports = primsMaze;
}