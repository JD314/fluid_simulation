
# -*- coding: utf-8 -*-
"""
compare_solvers_size.py

Compara la eficiencia de varios algoritmos de resolución de laberintos
(entrada arriba, salida abajo) desde tamaño 10x10 hacia arriba.

Algoritmos:
- WaterSolver (best-first con sesgo gravitacional: "ir más abajo")
- Left-hand Rule (regla de la mano izquierda)
- Right-hand Rule (regla de la mano derecha)
- BFS (camino más corto en pasos)
- A* (heurística Manhattan)
- Random Walk (base ineficiente)

Métrica:
    eficiencia = 1 - (celdas_visitadas / celdas_abiertas)

Requisitos: numpy, matplotlib (para el gráfico)
"""

from __future__ import annotations
import math
import random
from typing import Dict, List, Optional, Set, Tuple

import numpy as np
import matplotlib.pyplot as plt

Coord = Tuple[int, int]  # (fila, columna)


# ---------------------------------------------------------------------
# Utilidades de laberinto (entrada arriba, salida abajo)
# ---------------------------------------------------------------------

def ensure_odd(n: int) -> int:
    """Asegura tamaño impar para el generador."""
    return n if n % 2 == 1 else n + 1

def generate_maze(height: int = 41, width: int = 41, seed: int = 0) -> np.ndarray:
    """
    Genera un laberinto perfecto (DFS recursivo). 0=pared, 1=espacio.
    Abre entrada en fila 0 y salida en fila H-1, cerca del centro.
    height y width deben ser impares >= 5.
    """
    height = ensure_odd(max(5, height))
    width = ensure_odd(max(5, width))
    random.seed(seed)
    H, W = height, width
    grid = np.zeros((H, W), dtype=np.uint8)

    # Comenzar en (1,1) y tallar pasillos en celdas impares
    stack = [(1, 1)]
    grid[1, 1] = 1

    def neighbors_cells(r, c):
        for dr, dc in [(-2, 0), (2, 0), (0, -2), (0, 2)]:
            nr, nc = r + dr, c + dc
            if 1 <= nr < H - 1 and 1 <= nc < W - 1 and grid[nr, nc] == 0:
                yield nr, nc, (dr // 2, dc // 2)

    while stack:
        r, c = stack[-1]
        nbs = list(neighbors_cells(r, c))
        if not nbs:
            stack.pop()
            continue
        nr, nc, mid = random.choice(nbs)
        grid[r + mid[0], c + mid[1]] = 1
        grid[nr, nc] = 1
        stack.append((nr, nc))

    # Abrir entrada (arriba) y salida (abajo) cerca del centro
    def open_near_middle(row_idx: int) -> int:
        mid = W // 2
        best, bestdist = None, 10**9
        r = 1 if row_idx == 0 else H - 2
        for c in range(1, W - 1):
            if grid[r, c] == 1:
                d = abs(c - mid)
                if d < bestdist:
                    best, bestdist = c, d
        return best if best is not None else (1 if row_idx == 0 else W - 2)

    c_top = open_near_middle(0)
    c_bot = open_near_middle(H - 1)
    grid[0, c_top] = 1
    grid[H - 1, c_bot] = 1
    return grid

def neighbors4(grid: np.ndarray, r: int, c: int) -> List[Coord]:
    H, W = grid.shape
    res: List[Coord] = []
    for dr, dc in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
        nr, nc = r + dr, c + dc
        if 0 <= nr < H and 0 <= nc < W and grid[nr, nc] == 1:
            res.append((nr, nc))
    return res

def find_entrance_exit(grid: np.ndarray) -> Tuple[Coord, Coord]:
    H, W = grid.shape
    top = [c for c in range(W) if grid[0, c] == 1]
    bot = [c for c in range(W) if grid[H - 1, c] == 1]
    if not top or not bot:
        raise ValueError("Se requieren aperturas en fila 0 y fila H-1.")
    s = (0, top[len(top) // 2])
    g = (H - 1, bot[len(bot) // 2])
    return s, g


# ---------------------------------------------------------------------
# Resultados y base de solvers
# ---------------------------------------------------------------------

class SolveResult:
    def __init__(self, path: List[Coord], visited: Set[Coord], reached: bool):
        self.path = path
        self.visited = visited
        self.reached = reached
        self.visited_ratio: float = float("nan")  # se completa en evaluate

class BaseSolver:
    name = "base"
    def solve(self, grid: np.ndarray, start: Coord, goal: Coord) -> SolveResult:
        raise NotImplementedError


# ---------------------------------------------------------------------
# Solvers clásicos
# ---------------------------------------------------------------------

class BFSSolver(BaseSolver):
    name = "BFS (shortest-by-steps)"
    def solve(self, grid, start, goal):
        from collections import deque
        q = deque([start])
        parent: Dict[Coord, Optional[Coord]] = {start: None}
        visited: Set[Coord] = {start}
        while q:
            u = q.popleft()
            if u == goal:
                break
            for v in neighbors4(grid, *u):
                if v not in visited:
                    visited.add(v)
                    parent[v] = u
                    q.append(v)
        reached = goal in parent
        path: List[Coord] = []
        if reached:
            cur = goal
            while cur is not None:
                path.append(cur)
                cur = parent[cur]
            path.reverse()
        return SolveResult(path, visited, reached)

class AStarSolver(BaseSolver):
    name = "A* (Manhattan)"
    def solve(self, grid, start, goal):
        import heapq
        def h(a: Coord, b: Coord) -> int:
            return abs(a[0] - b[0]) + abs(a[1] - b[1])
        openh: List[Tuple[int, int, Coord, Optional[Coord]]] = []
        heapq.heappush(openh, (h(start, goal), 0, start, None))
        parent: Dict[Coord, Optional[Coord]] = {}
        gscore: Dict[Coord, int] = {start: 0}
        visited: Set[Coord] = set()
        while openh:
            f, g, u, p = heapq.heappop(openh)
            if u in visited:
                continue
            visited.add(u)
            parent[u] = p
            if u == goal:
                break
            for v in neighbors4(grid, *u):
                ng = g + 1
                if ng < gscore.get(v, 10**18):
                    gscore[v] = ng
                    heapq.heappush(openh, (ng + h(v, goal), ng, v, u))
        reached = goal in parent
        path: List[Coord] = []
        if reached:
            cur = goal
            while cur is not None:
                path.append(cur)
                cur = parent[cur]
            path.reverse()
        return SolveResult(path, visited, reached)

class RandomWalkSolver(BaseSolver):
    name = "Random Walk"
    def __init__(self, max_steps: int = 100000, seed: int = 0):
        self.max_steps = max_steps
        self.rng = random.Random(seed)
    def solve(self, grid, start, goal):
        cur = start
        visited: Set[Coord] = {cur}
        parent: Dict[Coord, Optional[Coord]] = {cur: None}
        steps = 0
        while steps < self.max_steps and cur != goal:
            steps += 1
            nbs = neighbors4(grid, *cur)
            if not nbs:
                break
            nxt = self.rng.choice(nbs)
            if nxt not in parent:
                parent[nxt] = cur
            cur = nxt
            visited.add(cur)
        reached = (cur == goal)
        path: List[Coord] = []
        if reached:
            curp = cur
            while curp is not None:
                path.append(curp)
                curp = parent[curp]
            path.reverse()
        return SolveResult(path, visited, reached)

class WallFollower(BaseSolver):
    """Regla de la mano (izquierda o derecha)."""
    name = "Wall Follower (hand rule)"
    def __init__(self, left: bool = True):
        self.left = left
        self.name = ("Left-" if left else "Right-") + "hand Rule"
    def solve(self, grid, start, goal):
        # direcciones: 0=arriba,1=derecha,2=abajo,3=izquierda
        def left_of(d): return (d - 1) % 4
        def right_of(d): return (d + 1) % 4
        def move(p, d):
            r, c = p
            if d == 0: return (r - 1, c)
            if d == 1: return (r, c + 1)
            if d == 2: return (r + 1, c)
            return (r, c - 1)
        def can_step(p):
            r, c = p
            H, W = grid.shape
            return 0 <= r < H and 0 <= c < W and grid[r, c] == 1

        heading = 2  # empezar "mirando hacia abajo"
        cur = start
        visited: Set[Coord] = {cur}
        parent: Dict[Coord, Optional[Coord]] = {cur: None}
        safety = 0
        while cur != goal and safety < grid.size * 10:
            safety += 1
            first = left_of(heading) if self.left else right_of(heading)
            choices = [first, heading, (first + 1) % 4, (first + 2) % 4]
            moved = False
            for d in choices:
                nxt = move(cur, d)
                if can_step(nxt):
                    if nxt not in parent:
                        parent[nxt] = cur
                    cur = nxt
                    heading = d
                    visited.add(cur)
                    moved = True
                    break
            if not moved:
                heading = (heading + 2) % 4  # dar la vuelta

        reached = (cur == goal)
        path: List[Coord] = []
        if reached:
            curp = cur
            while curp is not None:
                path.append(curp)
                curp = parent[curp]
            path.reverse()
        return SolveResult(path, visited, reached)


# ---------------------------------------------------------------------
# Solver "agua": best-first con sesgo gravitacional
# ---------------------------------------------------------------------

class WaterSolver(BaseSolver):
    """
    Solver "agua": prioriza ir más abajo (fila mayor), penaliza subir y
    penaliza levemente el desplazamiento lateral.
    """
    name = "Water (gravity-biased best-first)"
    def __init__(self, gravity_bias: float = 1.0, side_cost: float = 0.1, up_cost: float = 1.0):
        self.gbias = gravity_bias
        self.side_cost = side_cost
        self.up_cost = up_cost

    def solve(self, grid, start, goal):
        import heapq
        def h(a: Coord, b: Coord) -> int:
            return abs(a[0] - b[0]) + abs(a[1] - b[1])
        def move_penalty(u: Coord, v: Coord) -> float:
            dr = v[0] - u[0]; dc = v[1] - u[1]
            if dr > 0 and dc == 0:   # bajar
                return 0.0
            if dr == 0 and dc != 0:  # lateral
                return self.side_cost
            if dr < 0 and dc == 0:   # subir
                return self.up_cost
            return 0.5
        def priority(v: Coord) -> Tuple[float, float]:
            # prioridad principal: ir "más abajo" -> -row
            return (-self.gbias * v[0], h(v, goal))

        openh: List[Tuple[Tuple[float, float], float, Coord]] = []
        parent: Dict[Coord, Optional[Coord]] = {start: None}
        gcost: Dict[Coord, float] = {start: 0.0}
        visited: Set[Coord] = set()
        heapq.heappush(openh, (priority(start), 0.0, start))

        while openh:
            pr, g, u = heapq.heappop(openh)
            if u in visited:
                continue
            visited.add(u)
            if u == goal:
                break
            for v in neighbors4(grid, *u):
                newc = gcost[u] + 1.0 + move_penalty(u, v)
                if newc < gcost.get(v, 10**18):
                    gcost[v] = newc
                    parent[v] = u
                    heapq.heappush(openh, (priority(v), newc, v))

        reached = (goal in parent)
        path: List[Coord] = []
        if reached:
            cur = goal
            while cur is not None:
                path.append(cur)
                cur = parent[cur]
            path.reverse()
        return SolveResult(path, visited, reached)


# ---------------------------------------------------------------------
# Evaluación
# ---------------------------------------------------------------------

def evaluate_solver(grid: np.ndarray, solver: BaseSolver, start: Coord, goal: Coord) -> Dict[str, float]:
    res = solver.solve(grid, start, goal)
    open_cells = int(grid.sum())
    visited = len(res.visited)
    visited_ratio = visited / max(open_cells, 1)
    res.visited_ratio = visited_ratio
    path_len = len(res.path) if res.reached else math.inf
    return {
        "name": solver.name,
        "reached": 1.0 if res.reached else 0.0,
        "visited": visited,
        "open_cells": open_cells,
        "visited_ratio": visited_ratio,
        "efficiency": 1.0 - visited_ratio,
        "path_length": path_len,
    }


# ---------------------------------------------------------------------
# Main: empieza en 10x10, compara y grafica
# ---------------------------------------------------------------------

if __name__ == "__main__":
    # Rango de tamaños: desde 10x10 (ajustado a 11x11) en pasos de 10
    sizes_raw = list(range(10, 62, 5))  # pasos de 2 desde 10 hasta 60 (incluye 60)
    sizes = [ensure_odd(n) for n in sizes_raw]
    seeds = [1, 7, 11, 23, 37]  # promediaremos sobre estas semillas

    # Parámetros para el "agua"
    water_params = dict(gravity_bias=1.0, side_cost=0.1, up_cost=1.0)

    # Lista de solvers
    def make_solvers(seed: int) -> List[BaseSolver]:
        return [
            WaterSolver(**water_params),
            WallFollower(left=True),
            WallFollower(left=False),
            BFSSolver(),
            AStarSolver(),
            RandomWalkSolver(seed=seed, max_steps=100000),
        ]

    # Recopilar eficiencia promedio por tamaño y por algoritmo
    alg_names = None
    efficiencies: Dict[str, List[float]] = {}

    print("name\tsize\treached\tvisited\topen_cells\tvisited_ratio\tefficiency\tpath_length")

    for N in sizes:
        eff_accum: Dict[str, List[float]] = {}
        for sd in seeds:
            grid = generate_maze(N, N, seed=sd)
            start, goal = find_entrance_exit(grid)
            for solver in make_solvers(sd):
                metrics = evaluate_solver(grid, solver, start, goal)
                if alg_names is None:
                    alg_names = []
                if solver.name not in eff_accum:
                    eff_accum[solver.name] = []
                eff_accum[solver.name].append(metrics["efficiency"])
                # Línea detallada (una por semilla) para referencia
                print(
                    f"{solver.name}\t{N}x{N}\t{int(metrics['reached'])}\t"
                    f"{metrics['visited']}\t{metrics['open_cells']}\t"
                    f"{metrics['visited_ratio']:.3f}\t{metrics['efficiency']:.3f}\t"
                    f"{metrics['path_length'] if math.isfinite(metrics['path_length']) else 'inf'}"
                )
        # Promedio por tamaño
        for name, vals in eff_accum.items():
            if name not in efficiencies:
                efficiencies[name] = []
            efficiencies[name].append(float(np.mean(vals)))

    # --- Gráfico: eficiencia vs tamaño ---
    plt.figure(figsize=(10, 5))
    for name, effs in efficiencies.items():
        plt.plot(sizes, effs, alpha=0.4, label=name)  # línea más clara
        plt.scatter(sizes, effs, marker="x")    # puntos más reteñidos
    plt.xlabel("Tamaño del laberinto $(N × N)$")
    plt.ylabel("Eficiencia")
    plt.title("Eficiencia por algoritmo")
    plt.grid(True, linestyle="--", alpha=0.7)
    plt.legend()
    plt.tight_layout()
    plt.show()
