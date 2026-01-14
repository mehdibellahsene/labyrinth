namespace Labyrinth.Core;

using global::Labyrinth.Tiles;

public class Crawler : ICrawler
{
    private readonly global::Labyrinth.Labyrinth _maze;

    public int X { get; private set; }
    public int Y { get; private set; }
    public Direction Direction { get; private set; }

    public Crawler(global::Labyrinth.Labyrinth maze, int x, int y, Direction direction)
    {
        _maze = maze;
        X = x;
        Y = y;
        Direction = direction;
    }

    public Task<TileType> GetFacingTileTypeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var (dx, dy) = GetDirectionOffset();
        var tile = _maze.GetTile(X + dx, Y + dy);
        var type = tile switch
        {
            Wall => TileType.Wall,
            Door d => d.IsOpened ? TileType.Room : TileType.Door,
            Room => TileType.Room,
            _ => TileType.Wall
        };
        return Task.FromResult(type);
    }

    public Task<bool> TryWalkAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var (dx, dy) = GetDirectionOffset();
        var tile = _maze.GetTile(X + dx, Y + dy);

        if (tile is Wall || (tile is Door d && !d.IsOpened))
            return Task.FromResult(false);

        X += dx;
        Y += dy;
        return Task.FromResult(true);
    }

    public void TurnLeft() => Direction = (Direction)(((int)Direction + 3) % 4);
    public void TurnRight() => Direction = (Direction)(((int)Direction + 1) % 4);

    private (int dx, int dy) GetDirectionOffset() => Direction switch
    {
        Direction.North => (0, -1),
        Direction.East => (1, 0),
        Direction.South => (0, 1),
        Direction.West => (-1, 0),
        _ => (0, 0)
    };
}
