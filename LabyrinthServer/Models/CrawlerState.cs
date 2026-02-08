namespace LabyrinthServer.Models;

using ApiTypes;

public class CrawlerState
{
    public int Id { get; init; }
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Direction { get; set; }
    public List<string> Inventory { get; } = new();
}

public record WalkResult(bool Success, IEnumerable<string>? Items = null);
public record TileInfo(string Type);
