namespace Labyrinth.Exploration;

public record ExplorationStats
{
    public int TotalTilesDiscovered { get; init; }
    public int RoomsDiscovered { get; init; }
    public int DoorsDiscovered { get; init; }
    public int DoorsOpened { get; init; }
    public int KeysFound { get; init; }
    public int CrawlerCount { get; init; }
}
