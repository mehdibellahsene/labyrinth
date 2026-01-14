namespace Labyrinth.Core;

public interface ICrawler
{
    int X { get; }
    int Y { get; }
    Direction Direction { get; }

    Task<TileType> GetFacingTileTypeAsync(CancellationToken ct = default);
    Task<bool> TryWalkAsync(CancellationToken ct = default);
    void TurnLeft();
    void TurnRight();
}

public enum TileType { Wall, Room, Door }

public enum Direction { North, East, South, West }
