function aldousBroderMaze(width, height, startSide = 0, startCoord = 0, endSide = 3, endCoord = 0) {
    width -= width % 2; width++;
    height -= height % 2; height++;

    const maze = Array.from({ length: height }, (_, i) =>
        Array.from({ length: width }, (_, j) => {
            if (i % 2 === 1 && j % 2 === 1) return 1;
            return 1;
        })
    );

    let unvisited = Math.floor(height / 2) * Math.floor(width / 2);
    let on;
    do {
        on = [Math.floor(Math.random() * height), Math.floor(Math.random() * width)];
    } while (on[0] % 2 === 0 || on[1] % 2 === 0);

    maze[on[0]][on[1]] = 0;
    unvisited--;

    while (unvisited > 0) {
        const n = neighborsAB(maze, on[0], on[1]);
        const to = n[Math.floor(Math.random() * n.length)];
        if (maze[to[0]][to[1]] === 1) {
            maze[to[0]][to[1]] = 0;
            maze[(to[0] + on[0]) / 2][(to[1] + on[1]) / 2] = 0;
            unvisited--;
        }
        on = to;
    }

    const addOpening = (side, rel, value = 0) => {
        const clamp = (v, max) => Math.max(0, Math.min(max - 1, Math.floor(v)));
        switch (side) {
            case 0: maze[0][clamp(1 + (width - 3) * rel, width)] = value; break;
            case 1: maze[clamp(1 + (height - 3) * rel, height)][width - 1] = value; break;
            case 2: maze[clamp(1 + (height - 3) * rel, height)][0] = value; break;
            case 3: maze[height - 1][clamp(1 + (width - 3) * rel, width)] = value; break;
        }
    };

    addOpening(startSide, startCoord);
    addOpening(endSide, endCoord);

    return maze;
}

function neighborsAB(maze, ic, jc) {
    const final = [];
    for (let i = 0; i < 4; i++) {
        const n = [ic, jc];
        n[i % 2] += ((Math.floor(i / 2) * 2) || -2);
        if (n[0] < maze.length && n[1] < maze[0].length && n[0] > 0 && n[1] > 0) {
            final.push(n);
        }
    }
    return final;
}

// Permitir que se ejecute como script desde la terminal o se importe como módulo
if (require.main === module) {
    const [,, width, height, startSide, startCoord, endSide, endCoord] = process.argv;
    const result = aldousBroderMaze(
        parseInt(width),
        parseInt(height),
        parseInt(startSide),
        parseFloat(startCoord),
        parseInt(endSide),
        parseFloat(endCoord)
    );
    console.log(result);
} else {
    module.exports = aldousBroderMaze;
}
