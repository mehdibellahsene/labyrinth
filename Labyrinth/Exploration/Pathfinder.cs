using Labyrinth.Crawl;

namespace Labyrinth.Exploration;

public class Pathfinder(ISharedMap map) : IPathfinder
{
    private static readonly (int dx, int dy, Direction dir)[] Directions =
    [
        (0, -1, Direction.North), (0, 1, Direction.South),
        (-1, 0, Direction.West), (1, 0, Direction.East)
    ];

    public PathResult FindPath(int startX, int startY, int targetX, int targetY, bool hasKey = false) =>
        FindNearest(startX, startY, (_, x, y) => x == targetX && y == targetY, hasKey);

    public PathResult FindNearest(int startX, int startY, Func<MapTile?, int, int, bool> predicate, bool hasKey = false)
    {
        var visited = new HashSet<(int, int)> { (startX, startY) };
        var parents = new Dictionary<(int, int), ((int, int) parent, Direction dir)>();
        var queue = new Queue<(int x, int y)>();
        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            var tile = map.GetTile(x, y);

            if (parents.ContainsKey((x, y)) && predicate(tile, x, y))
                return PathResult.Success(ReconstructPath(parents, (startX, startY), (x, y)), (x, y));

            bool isStart = x == startX && y == startY;
            if (!isStart && (tile == null || !map.IsTraversable(x, y, hasKey)))
                continue;

            foreach (var (dx, dy, dir) in Directions)
            {
                var (nx, ny) = (x + dx, y + dy);
                if (!visited.Add((nx, ny))) continue;

                var neighbor = map.GetTile(nx, ny);
                if (neighbor is { IsWall: true }) continue;

                parents[(nx, ny)] = ((x, y), dir);
                queue.Enqueue((nx, ny));
            }
        }

        return PathResult.Failure((0, 0));
    }

    private static List<Direction> ReconstructPath(
        Dictionary<(int, int), ((int, int) parent, Direction dir)> parents,
        (int, int) start, (int, int) target)
    {
        var path = new List<Direction>();
        for (var cur = target; cur != start;)
        {
            var (parent, dir) = parents[cur];
            path.Add(dir);
            cur = parent;
        }
        path.Reverse();
        return path;
    }

    public PathResult FindNearestUnknown(int startX, int startY, bool hasKey = false) =>
        FindNearest(startX, startY, (tile, _, _) => tile == null, hasKey);

    public PathResult FindNearestKey(int startX, int startY) =>
        FindNearest(startX, startY, (tile, _, _) => tile?.HasKey == true);

    public PathResult FindNearestDoor(int startX, int startY, bool hasKey) =>
        FindNearest(startX, startY, (tile, _, _) => tile is { IsDoor: true, IsDoorOpen: false }, hasKey);

    public PathResult FindExit(int startX, int startY, bool hasKey = false) =>
        FindNearest(startX, startY, (tile, _, _) => tile?.IsOutside == true, hasKey);
}
