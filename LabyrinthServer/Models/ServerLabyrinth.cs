using ApiTypes;

namespace LabyrinthServer.Models;

public class ServerLabyrinth
{
    private readonly TileType[,] _tiles;
    private readonly Dictionary<(int, int), ServerDoor> _doors = new();
    private readonly Dictionary<(int, int), List<InventoryItem>> _roomItems = new();
    private readonly object _itemLock = new();

    public int Width { get; }
    public int Height { get; }
    public int StartX { get; }
    public int StartY { get; }

    public ServerLabyrinth(string asciiMap)
    {
        var lines = asciiMap.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.TrimEnd('\r')).ToArray();

        Height = lines.Length;
        Width = lines.Max(l => l.Length);
        _tiles = new TileType[Width, Height];

        var doorPos = new List<(int, int)>();
        var keyPos = new List<(int, int)>();

        for (int y = 0; y < Height; y++)
        {
            var line = y < lines.Length ? lines[y] : "";
            for (int x = 0; x < Width; x++)
            {
                char c = x < line.Length ? line[x] : ' ';
                _tiles[x, y] = c switch
                {
                    '#' or '+' or '-' or '|' => TileType.Wall,
                    '/' => TileType.Door,
                    'x' or 'k' or ' ' => TileType.Room,
                    _ => TileType.Wall
                };

                if (c == 'x') { StartX = x; StartY = y; }
                else if (c == '/') doorPos.Add((x, y));
                else if (c == 'k') keyPos.Add((x, y));
            }
        }

        for (int i = 0; i < doorPos.Count && i < keyPos.Count; i++)
        {
            _doors[doorPos[i]] = new ServerDoor(Guid.NewGuid()) { RequiredKeyIndex = i };
            _roomItems[keyPos[i]] = [new InventoryItem { Type = ItemType.Key, MoveRequired = false }];
        }
    }

    public TileType GetTileType(int x, int y) =>
        x < 0 || x >= Width || y < 0 || y >= Height ? TileType.Outside : _tiles[x, y];

    public ServerDoor? GetDoor(int x, int y) =>
        _doors.TryGetValue((x, y), out var door) ? door : null;

    public List<InventoryItem> GetRoomItems(int x, int y)
    {
        lock (_itemLock)
            return _roomItems.TryGetValue((x, y), out var items) ? items.ToList() : [];
    }

    public bool TryMoveItemsFromRoom(int x, int y, List<bool> moves, List<InventoryItem> targetBag)
    {
        lock (_itemLock)
        {
            if (!_roomItems.TryGetValue((x, y), out var items)) items = [];
            if (moves.Count != items.Count) return false;
            for (int i = items.Count - 1; i >= 0; i--)
                if (moves[i]) { targetBag.Add(items[i]); items.RemoveAt(i); }
            return true;
        }
    }

    public bool TryMoveItemsToRoom(int x, int y, List<bool> moves, List<InventoryItem> sourceBag)
    {
        lock (_itemLock)
        {
            if (!_roomItems.ContainsKey((x, y))) _roomItems[(x, y)] = [];
            if (moves.Count != sourceBag.Count) return false;
            for (int i = sourceBag.Count - 1; i >= 0; i--)
                if (moves[i]) { _roomItems[(x, y)].Add(sourceBag[i]); sourceBag.RemoveAt(i); }
            return true;
        }
    }
}

public class ServerDoor(Guid keyId)
{
    public Guid KeyId { get; } = keyId;
    public bool IsOpen { get; private set; }
    public int RequiredKeyIndex { get; set; }

    public bool TryOpen(InventoryItem key)
    {
        if (key.Type != ItemType.Key) return false;
        IsOpen = true;
        return true;
    }
}
