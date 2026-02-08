using System.Collections.Concurrent;
using ApiTypes;
using LabyrinthServer.Models;

namespace LabyrinthServer.Services;

public class LabyrinthService : ILabyrinthService
{
    private const int MaxCrawlersPerApp = 3;
    private readonly ConcurrentDictionary<Guid, ServerCrawler> _crawlers = new();
    private readonly ConcurrentDictionary<Guid, ServerLabyrinth> _labyrinths = new();
    private readonly SemaphoreSlim _createLock = new(1, 1);

    private static readonly string[] Maps =
    [
        """
        +------/------+
        |      k      |
        |  +---/---+  |
        |  |   k   |  |
        |  | +---+ |  |
        |  | |kx | |  |
        |  | +-/-+ |  |
        |  |       |  |
        |  +-------+  |
        |             |
        +-------------+
        """,
        """
        +---------/---------+
        |         k         |
        |  +------/------+  |
        |  |      k      |  |
        |  |  +---/---+  |  |
        |  |  |   k   |  |  |
        |  |  | +---+ |  |  |
        |  |  | |kx | |  |  |
        |  |  | +-/-+ |  |  |
        |  |  |       |  |  |
        |  |  +-------+  |  |
        |  |             |  |
        |  +-------------+  |
        |                   |
        +-------------------+
        """,
        """
        +------------/------------+
        |            k            |
        |  +---------/---------+  |
        |  |         k         |  |
        |  |  +------/------+  |  |
        |  |  |      k      |  |  |
        |  |  |  +---/---+  |  |  |
        |  |  |  |   k   |  |  |  |
        |  |  |  | +---+ |  |  |  |
        |  |  |  | |kx | |  |  |  |
        |  |  |  | +-/-+ |  |  |  |
        |  |  |  |       |  |  |  |
        |  |  |  +-------+  |  |  |
        |  |  |             |  |  |
        |  |  +-------------+  |  |
        |  |                   |  |
        |  +-------------------+  |
        |                         |
        +-------------------------+
        """
    ];

    public IEnumerable<Crawler> GetCrawlers(Guid appKey)
    {
        var lab = GetOrCreateLabyrinth(appKey);
        return _crawlers.Values.Where(c => c.AppKey == appKey).Select(c => c.ToDto(lab));
    }

    public async Task<Crawler?> CreateCrawler(Guid appKey, Settings? settings)
    {
        await _createLock.WaitAsync();
        try
        {
            if (_crawlers.Values.Count(c => c.AppKey == appKey) >= MaxCrawlersPerApp) return null;
            var lab = GetOrCreateLabyrinth(appKey, settings);
            var crawler = new ServerCrawler { AppKey = appKey, X = lab.StartX, Y = lab.StartY };
            _crawlers[crawler.Id] = crawler;
            return crawler.ToDto(lab);
        }
        finally { _createLock.Release(); }
    }

    public Crawler? GetCrawler(Guid appKey, Guid crawlerId)
    {
        if (!_crawlers.TryGetValue(crawlerId, out var c) || c.AppKey != appKey) return null;
        return c.ToDto(GetOrCreateLabyrinth(appKey));
    }

    public (Crawler? crawler, bool walkFailed) UpdateCrawler(Guid appKey, Guid crawlerId, Crawler update)
    {
        if (!_crawlers.TryGetValue(crawlerId, out var c) || c.AppKey != appKey) return (null, false);
        var lab = GetOrCreateLabyrinth(appKey);
        c.Direction = update.Dir;
        bool walkFailed = update.Walking && !c.TryWalk(lab);
        return (c.ToDto(lab), walkFailed);
    }

    public bool DeleteCrawler(Guid appKey, Guid crawlerId)
    {
        if (!_crawlers.TryGetValue(crawlerId, out var c) || c.AppKey != appKey) return false;
        return _crawlers.TryRemove(crawlerId, out _);
    }

    public IEnumerable<InventoryItem>? GetBag(Guid appKey, Guid crawlerId) =>
        _crawlers.TryGetValue(crawlerId, out var c) && c.AppKey == appKey ? c.Bag.ToList() : null;

    public IEnumerable<InventoryItem>? UpdateBag(Guid appKey, Guid crawlerId, IEnumerable<InventoryItem> items)
    {
        if (!_crawlers.TryGetValue(crawlerId, out var c) || c.AppKey != appKey) return null;
        var lab = GetOrCreateLabyrinth(appKey);
        var moves = items.Select(i => i.MoveRequired ?? false).ToList();
        return lab.TryMoveItemsToRoom(c.X, c.Y, moves, c.Bag) ? c.Bag.ToList() : null;
    }

    public IEnumerable<InventoryItem>? GetItems(Guid appKey, Guid crawlerId) =>
        _crawlers.TryGetValue(crawlerId, out var c) && c.AppKey == appKey
            ? GetOrCreateLabyrinth(appKey).GetRoomItems(c.X, c.Y) : null;

    public IEnumerable<InventoryItem>? UpdateItems(Guid appKey, Guid crawlerId, IEnumerable<InventoryItem> items)
    {
        if (!_crawlers.TryGetValue(crawlerId, out var c) || c.AppKey != appKey) return null;
        var lab = GetOrCreateLabyrinth(appKey);
        var moves = items.Select(i => i.MoveRequired ?? false).ToList();
        return lab.TryMoveItemsFromRoom(c.X, c.Y, moves, c.Bag) ? lab.GetRoomItems(c.X, c.Y) : null;
    }

    public bool HasAccess(Guid appKey, Guid crawlerId) =>
        _crawlers.TryGetValue(crawlerId, out var c) && c.AppKey == appKey;

    public bool CrawlerExists(Guid crawlerId) => _crawlers.ContainsKey(crawlerId);

    private ServerLabyrinth GetOrCreateLabyrinth(Guid appKey, Settings? settings = null) =>
        _labyrinths.GetOrAdd(appKey, _ => new ServerLabyrinth(
            Maps[settings?.RandomSeed != null ? Math.Abs(settings.RandomSeed.Value) % Maps.Length : 0]));
}
