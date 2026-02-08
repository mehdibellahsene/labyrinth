using Labyrinth.Tiles;

namespace Labyrinth.Exploration;

public class MapTile(int x, int y, Type tileType, Guid discoveredBy)
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public Type TileType { get; } = tileType;
    public bool IsTraversable { get; } = tileType != typeof(Wall) && tileType != typeof(Outside);
    public bool HasKey { get; set; }
    public bool IsDoorOpen { get; set; }
    public Guid DiscoveredBy { get; } = discoveredBy;

    public bool IsUnknown => TileType == typeof(Unknown);
    public bool IsRoom => TileType == typeof(Room);
    public bool IsDoor => TileType == typeof(Door);
    public bool IsWall => TileType == typeof(Wall);
    public bool IsOutside => TileType == typeof(Outside);
}
