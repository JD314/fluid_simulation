function backtrackingMaze(width, height, startSide = 0, startRel = 0.5, endSide = 0, endRel = 0.5) {
    width -= width % 2; width++;
    height -= height % 2; height++;

    var maze = Array.from({ length: height }, () => Array(width).fill(1));

    // Aplica coordenadas de entrada y salida
    function getEdgeCoordinates(side, rel) {
        const clamp = (v, max) => Math.max(1, Math.min(max - 2, Math.floor(rel * (max - 2) / 2) * 2 + 1));
        if (side === 0) return [height - 1, clamp(rel, width)];
        if (side === 1) return [clamp(rel, height), width - 1];
        if (side === 2) return [clamp(rel, height), 0];
        if (side === 3) return [0, clamp(rel, width)];
        throw new Error("Invalid side");
    }

    const [start_i, start_j] = getEdgeCoordinates(startSide, startRel);
    const [end_i, end_j] = getEdgeCoordinates(endSide, endRel);

    maze[start_i][start_j] = 0;

    var start = [];
    do { start[0] = Math.floor(Math.random() * height); } while (start[0] % 2 === 0);
    do { start[1] = Math.floor(Math.random() * width); } while (start[1] % 2 === 0);
    maze[start[0]][start[1]] = 0;

    var openCells = [start];

    while (openCells.length) {
        var cell, n;
        openCells.push([-1, -1]);
        do {
            openCells.pop();
            if (openCells.length === 0) break;
            cell = openCells[openCells.length - 1];
            n = neighbors(maze, cell[0], cell[1]);
        } while (n.length === 0 && openCells.length > 0);

        if (openCells.length === 0) break;

        var choice = n[Math.floor(Math.random() * n.length)];
        openCells.push(choice);
        maze[choice[0]][choice[1]] = 0;
        maze[(choice[0] + cell[0]) / 2][(choice[1] + cell[1]) / 2] = 0;
    }

    maze[end_i][end_j] = 0;
    return maze;
}

function neighbors(maze, ic, jc) {
    var final = [];
    for (var i = 0; i < 4; i++) {
        var n = [ic, jc];
        n[i % 2] += ((Math.floor(i / 2) * 2) || -2);
        if (n[0] < maze.length && n[1] < maze[0].length && n[0] > 0 && n[1] > 0) {
            if (maze[n[0]][n[1]] === 1) final.push(n);
        }
    }
    return final;
}

// Permitir que se ejecute como script desde la terminal o se importe como módulo
if (require.main === module) {
    const [,, width, height, startSide, startRel, endSide, endRel] = process.argv;
    const result = backtrackingMaze(
        parseInt(width),
        parseInt(height),
        parseInt(startSide),
        parseFloat(startRel),
        parseInt(endSide),
        parseFloat(endRel)
    );
    console.log(result);
} else {
    module.exports = backtrackingMaze;
}
