namespace Labyrinth.Exploration;

public interface IPathfinder
{
    PathResult FindPath(int startX, int startY, int targetX, int targetY, bool hasKey = false);
    PathResult FindNearest(int startX, int startY, Func<MapTile?, int, int, bool> predicate, bool hasKey = false);
}
