using System.Collections.Concurrent;
using Labyrinth.Tiles;

namespace Labyrinth.Exploration;

public class SharedMap : ISharedMap
{
    private readonly ConcurrentDictionary<(int, int), MapTile> _tiles = new();

    public MapTile? GetTile(int x, int y) =>
        _tiles.TryGetValue((x, y), out var tile) ? tile : null;

    public void UpdateTile(int x, int y, Type tileType, Guid crawlerId) =>
        _tiles.AddOrUpdate((x, y),
            _ => new MapTile(x, y, tileType, crawlerId),
            (_, existing) => existing.IsUnknown ? new MapTile(x, y, tileType, crawlerId) : existing);

    public void MarkKeyFound(int x, int y)
    {
        if (_tiles.TryGetValue((x, y), out var tile)) tile.HasKey = true;
    }

    public void MarkKeyCollected(int x, int y)
    {
        if (_tiles.TryGetValue((x, y), out var tile)) tile.HasKey = false;
    }

    public void MarkDoorOpened(int x, int y)
    {
        if (_tiles.TryGetValue((x, y), out var tile)) tile.IsDoorOpen = true;
    }

    public IEnumerable<(int X, int Y)> GetFrontierTiles()
    {
        var frontier = new HashSet<(int, int)>();
        (int dx, int dy)[] dirs = [(0, -1), (0, 1), (-1, 0), (1, 0)];

        foreach (var (pos, tile) in _tiles)
        {
            if (!tile.IsTraversable || tile.IsUnknown) continue;
            foreach (var (dx, dy) in dirs)
            {
                var n = (pos.Item1 + dx, pos.Item2 + dy);
                if (!_tiles.ContainsKey(n)) frontier.Add(n);
            }
        }
        return frontier;
    }

    public IEnumerable<MapTile> GetKeyTiles() =>
        _tiles.Values.Where(t => t.HasKey).ToList();

    public IEnumerable<MapTile> GetClosedDoors() =>
        _tiles.Values.Where(t => t.IsDoor && !t.IsDoorOpen).ToList();

    public IReadOnlyDictionary<(int, int), MapTile> GetAllTiles() =>
        new Dictionary<(int, int), MapTile>(_tiles);

    public bool IsTraversable(int x, int y, bool hasKey = false)
    {
        if (!_tiles.TryGetValue((x, y), out var tile)) return false;
        if (tile.IsWall || tile.IsOutside) return false;
        return !tile.IsDoor || tile.IsDoorOpen || hasKey;
    }
}
