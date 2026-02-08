using ApiTypes;

namespace LabyrinthServer.Models;

public class ServerCrawler
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AppKey { get; init; }
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Direction { get; set; } = Direction.North;
    public List<InventoryItem> Bag { get; } = new();

    private readonly object _lock = new();

    public TileType GetFacingTileType(ServerLabyrinth labyrinth)
    {
        var (dx, dy) = GetDirectionDelta();
        return labyrinth.GetTileType(X + dx, Y + dy);
    }

    public (int dx, int dy) GetDirectionDelta() => Direction switch
    {
        Direction.North => (0, -1),
        Direction.South => (0, 1),
        Direction.East => (1, 0),
        Direction.West => (-1, 0),
        _ => (0, 0)
    };

    public bool TryWalk(ServerLabyrinth labyrinth)
    {
        lock (_lock)
        {
            var (dx, dy) = GetDirectionDelta();
            var (newX, newY) = (X + dx, Y + dy);
            var tileType = labyrinth.GetTileType(newX, newY);

            if (tileType is TileType.Wall or TileType.Outside) return false;

            if (tileType == TileType.Door)
            {
                var door = labyrinth.GetDoor(newX, newY);
                if (door is { IsOpen: false })
                {
                    var keyIndex = Bag.FindIndex(i => i.Type == ItemType.Key);
                    if (keyIndex >= 0 && door.TryOpen(Bag[keyIndex]))
                        Bag.RemoveAt(keyIndex);
                    else
                        return false;
                }
            }

            X = newX;
            Y = newY;
            return true;
        }
    }

    public Crawler ToDto(ServerLabyrinth labyrinth) => new()
    {
        Id = Id, X = X, Y = Y, Dir = Direction, Walking = false,
        FacingTile = GetFacingTileType(labyrinth),
        Bag = Bag.ToArray(),
        Items = labyrinth.GetRoomItems(X, Y).ToArray()
    };
}
