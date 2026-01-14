namespace LabyrinthServer.Services;

using System.Collections.Concurrent;
using global::Labyrinth.Core;
using LabyrinthServer.Models;

public interface ILabyrinthService
{
    string[] GetMaze();
    int CreateCrawler();
    TileInfo? GetFacingTile(int id);
    bool TurnCrawler(int id, bool left);
    WalkResult TryWalk(int id);
    IEnumerable<string>? GetInventory(int id);
}

public class LabyrinthService : ILabyrinthService
{
    private static readonly string[] Maze =
    {
        "+-------+",
        "|   | k |",
        "+ +-+---+",
        "|   /   |",
        "+---+ +-+",
        "| x     |",
        "+-------+"
    };

    private readonly ConcurrentDictionary<int, CrawlerState> _crawlers = new();
    private readonly ConcurrentDictionary<(int, int), string?> _items = new();
    private int _nextId;

    public LabyrinthService()
    {
        _items[(4, 1)] = "key_1";
    }

    public string[] GetMaze() => Maze;

    public int CreateCrawler()
    {
        var id = Interlocked.Increment(ref _nextId);
        _crawlers[id] = new CrawlerState
        {
            Id = id,
            X = 2,
            Y = 5,
            Direction = Direction.North
        };
        return id;
    }

    public TileInfo? GetFacingTile(int id)
    {
        if (!_crawlers.TryGetValue(id, out var state)) return null;

        var (dx, dy) = GetOffset(state.Direction);
        var ch = GetCharAt(state.X + dx, state.Y + dy);

        var type = ch switch
        {
            ' ' or 'k' or 'x' => "room",
            '/' => "door",
            _ => "wall"
        };

        return new TileInfo(type);
    }

    public bool TurnCrawler(int id, bool left)
    {
        if (!_crawlers.TryGetValue(id, out var state)) return false;

        state.Direction = left
            ? (Direction)(((int)state.Direction + 3) % 4)
            : (Direction)(((int)state.Direction + 1) % 4);
        return true;
    }

    public WalkResult TryWalk(int id)
    {
        if (!_crawlers.TryGetValue(id, out var state))
            return new WalkResult(false);

        var (dx, dy) = GetOffset(state.Direction);
        var nx = state.X + dx;
        var ny = state.Y + dy;
        var ch = GetCharAt(nx, ny);

        if (ch == '|' || ch == '-' || ch == '+')
            return new WalkResult(false);

        if (ch == '/' && !state.Inventory.Any(k => k.StartsWith("key")))
            return new WalkResult(false);

        state.X = nx;
        state.Y = ny;

        if (_items.TryRemove((nx, ny), out var item) && item != null)
        {
            state.Inventory.Add(item);
            return new WalkResult(true, state.Inventory);
        }

        return new WalkResult(true);
    }

    public IEnumerable<string>? GetInventory(int id)
        => _crawlers.TryGetValue(id, out var s) ? s.Inventory : null;

    private static (int, int) GetOffset(Direction d) => d switch
    {
        Direction.North => (0, -1),
        Direction.East => (1, 0),
        Direction.South => (0, 1),
        Direction.West => (-1, 0),
        _ => (0, 0)
    };

    private char GetCharAt(int x, int y)
        => y >= 0 && y < Maze.Length && x >= 0 && x < Maze[y].Length
            ? Maze[y][x]
            : '#';
}
