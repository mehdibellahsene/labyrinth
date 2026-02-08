namespace Labyrinth.Exploration;

public interface ISharedMap
{
    MapTile? GetTile(int x, int y);
    void UpdateTile(int x, int y, Type tileType, Guid crawlerId);
    void MarkKeyFound(int x, int y);
    void MarkKeyCollected(int x, int y);
    void MarkDoorOpened(int x, int y);
    IEnumerable<(int X, int Y)> GetFrontierTiles();
    IEnumerable<MapTile> GetKeyTiles();
    IEnumerable<MapTile> GetClosedDoors();
    IReadOnlyDictionary<(int, int), MapTile> GetAllTiles();
    bool IsTraversable(int x, int y, bool hasKey = false);
}
