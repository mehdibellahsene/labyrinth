using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace Labyrinth.Exploration;

public class CoordinatedExplorer(ISharedMap? sharedMap = null)
{
    private readonly ISharedMap _sharedMap = sharedMap ?? new SharedMap();
    private readonly List<(SmartExplorer explorer, Inventory bag)> _entries = new();
    private readonly object _lock = new();

    public SmartExplorer AddCrawler(ICrawler crawler, Inventory? bag = null)
    {
        var explorer = new SmartExplorer(crawler, _sharedMap);
        bag ??= new MyInventory();
        lock (_lock) { _entries.Add((explorer, bag)); }
        return explorer;
    }

    public async Task<SmartExplorer?> ExploreAll(int maxSteps)
    {
        List<(SmartExplorer explorer, Inventory bag)> snapshot;
        lock (_lock) { snapshot = _entries.ToList(); }

        using var cts = new CancellationTokenSource();
        var tasks = snapshot.Select(e => Task.Run(async () =>
        {
            var remaining = await e.explorer.Explore(maxSteps, e.bag);
            var found = await e.explorer.Crawler.FacingTileType == typeof(Outside);
            if (found) cts.Cancel();
            return (e.explorer, found);
        })).ToArray();

        var results = await Task.WhenAll(tasks);
        return results.FirstOrDefault(r => r.found).explorer;
    }

    public async Task<int> ExploreCoordinated(int maxSteps)
    {
        List<(SmartExplorer explorer, Inventory bag)> snapshot;
        lock (_lock) { snapshot = _entries.ToList(); }

        using var cts = new CancellationTokenSource();
        var tasks = snapshot.Select(e => RunExplorer(e.explorer, maxSteps, e.bag, cts.Token)).ToList();

        while (tasks.Count > 0)
        {
            var done = await Task.WhenAny(tasks);
            tasks.Remove(done);
            var result = await done;
            if (result.found)
            {
                cts.Cancel();
                return result.steps;
            }
        }
        return 0;
    }

    private static async Task<(SmartExplorer explorer, bool found, int steps)> RunExplorer(
        SmartExplorer explorer, int maxSteps, Inventory bag, CancellationToken token)
    {
        int steps = maxSteps;
        while (steps > 0)
        {
            if (token.IsCancellationRequested) return (explorer, false, steps);
            if (await explorer.Crawler.FacingTileType == typeof(Outside)) return (explorer, true, steps);
            if (!await explorer.Step(bag)) break;
            steps--;
        }
        var found = await explorer.Crawler.FacingTileType == typeof(Outside);
        return (explorer, found, steps);
    }

    public ExplorationStats GetStats()
    {
        var tiles = _sharedMap.GetAllTiles();
        return new ExplorationStats
        {
            TotalTilesDiscovered = tiles.Count,
            RoomsDiscovered = tiles.Values.Count(t => t.IsRoom),
            DoorsDiscovered = tiles.Values.Count(t => t.IsDoor),
            DoorsOpened = tiles.Values.Count(t => t.IsDoor && t.IsDoorOpen),
            KeysFound = tiles.Values.Count(t => t.HasKey),
            CrawlerCount = _entries.Count
        };
    }
}
