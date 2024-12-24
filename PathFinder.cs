using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using GameOffsets2.Native;

namespace WhereAreYouGoing;

public class PathFinder
{
    private readonly bool[][] _grid;
    private readonly int _dimension2;
    private readonly int _dimension1;

    public PathFinder(int[][] grid, int[] pathableValues)
    {
        var pv = pathableValues.ToHashSet();
        _grid = grid.Select(x => x.Select(y => pv.Contains(y)).ToArray()).ToArray();
        _dimension1 = _grid.Length;
        _dimension2 = _grid[0].Length;
    }

    private bool IsTilePathable(Vector2i tile)
    {
        if (tile.X < 0 || tile.X >= _dimension2)
        {
            return false;
        }

        if (tile.Y < 0 || tile.Y >= _dimension1)
        {
            return false;
        }

        return _grid[tile.Y][tile.X];
    }

    private static readonly List<Vector2i> NeighborOffsets = new List<Vector2i>
    {
        new Vector2i(0, 1),
        new Vector2i(1, 1),
        new Vector2i(1, 0),
        new Vector2i(1, -1),
        new Vector2i(0, -1),
        new Vector2i(-1, -1),
        new Vector2i(-1, 0),
        new Vector2i(-1, 1),
    };

    private static IEnumerable<Vector2i> GetNeighbors(Vector2i tile)
    {
        return NeighborOffsets.Select(offset => tile + offset);
    }

    public List<Vector2i> FindPath(Vector2i start, Vector2i target)
    {
        if (!IsTilePathable(start) || !IsTilePathable(target))
            return null;

        var visited = new HashSet<Vector2i>();
        var cameFrom = new Dictionary<Vector2i, Vector2i>();
        var gScore = new Dictionary<Vector2i, float>();
        var fScore = new Dictionary<Vector2i, float>();
        var openSet = new BinaryHeap<float, Vector2i>();

        gScore[start] = 0;
        fScore[start] = start.DistanceF(target);
        openSet.Add(fScore[start], start);

        while (openSet.Count > 0)
        {
            var current = openSet.RemoveTop().Value;

            if (current == target)
            {
                var path = new List<Vector2i>();
                while (current != start)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            visited.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!IsTilePathable(neighbor) || visited.Contains(neighbor))
                    continue;

                var tentativeGScore = gScore[current] + current.DistanceF(neighbor);

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + neighbor.DistanceF(target);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(fScore[neighbor], neighbor);
                }
            }
        }

        return null; // No path found
    }
}
