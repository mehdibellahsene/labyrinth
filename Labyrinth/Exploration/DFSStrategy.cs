namespace Labyrinth.Exploration;

using global::Labyrinth.Core;

public class DFSStrategy : IExplorationStrategy
{
    private readonly Stack<(int x, int y)> _path = new();
    private readonly HashSet<(int x, int y)> _visited = new();
    private (int x, int y)? _backtrackTarget;

    public async Task<Actions> GetNextActionAsync(ICrawler crawler, CancellationToken ct = default)
    {
        var current = (crawler.X, crawler.Y);

        if (!_visited.Contains(current))
        {
            _visited.Add(current);
            _path.Push(current);
        }

        var frontType = await crawler.GetFacingTileTypeAsync(ct);
        var frontPos = GetPositionInFront(crawler);

        if (frontType != TileType.Wall && !_visited.Contains(frontPos))
            return Actions.Walk;

        return Actions.TurnRight;
    }

    public void Reset()
    {
        _path.Clear();
        _visited.Clear();
        _backtrackTarget = null;
    }

    private static (int x, int y) GetPositionInFront(ICrawler c) => c.Direction switch
    {
        Direction.North => (c.X, c.Y - 1),
        Direction.East => (c.X + 1, c.Y),
        Direction.South => (c.X, c.Y + 1),
        Direction.West => (c.X - 1, c.Y),
        _ => (c.X, c.Y)
    };
}
