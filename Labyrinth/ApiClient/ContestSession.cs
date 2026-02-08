using Labyrinth.Build;
using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Tiles;
using Labyrinth.Exploration;
using System.Net.Http.Json;
using Dto = ApiTypes;

namespace Labyrinth.ApiClient;

public class ContestSession
{
    private readonly HttpClient _http;
    private readonly Guid _appKey;
    private readonly RemoteContestLabyrinthBuilder _builder;
    private readonly IList<(ClientCrawler Crawler, Inventory Bag)> _crawlers;
    private readonly SemaphoreSlim _crawlerLock = new(1, 1);
    private readonly SharedMap _sharedMap = new();
    private int _callsToNewCrawler;

    public IEnumerable<ICrawler> Crawlers => _crawlers.Select(c => c.Crawler);
    public IEnumerable<Inventory> Bags => _crawlers.Select(c => c.Bag);
    public IBuilder Builder => _builder;
    public ISharedMap SharedMap => _sharedMap;

    private ContestSession(HttpClient http, Guid appKey, Dto.Crawler crawler)
    {
        _http = http;
        _appKey = appKey;
        _crawlers = new List<(ClientCrawler, Inventory)> { NewCrawlerAndItsBag(appKey, crawler) };
        _builder = new(_crawlers[0].Crawler);
    }

    public static async Task<ContestSession> Open(Uri serverUrl, Guid appKey, Dto.Settings? settings = null)
    {
        var http = new HttpClient { BaseAddress = serverUrl };
        return await CreateCrawler(http, appKey, settings) is Dto.Crawler dto
            ? new ContestSession(http, appKey, dto)
            : throw new FormatException("Failed to read a crawler");
    }

    public async Task Close()
    {
        await Task.WhenAll(_crawlers.Select(c => c.Crawler.Delete()));
        _crawlers.Clear();
    }

    public async Task<ICrawler> NewCrawler()
    {
        await _crawlerLock.WaitAsync();
        try
        {
            if (_callsToNewCrawler > 0)
                _crawlers.Add(NewCrawlerAndItsBag(_appKey, await CreateCrawler(_http, _appKey)));
            return _crawlers[_callsToNewCrawler++].Crawler;
        }
        finally { _crawlerLock.Release(); }
    }

    private (ClientCrawler, Inventory) NewCrawlerAndItsBag(Guid appKey, Dto.Crawler dto)
    {
        var crawler = new ClientCrawler(_http.BaseAddress!, appKey, dto, out var inventory);
        crawler.Changed += Crawler_Changed;
        return (crawler, inventory);
    }

    private async void Crawler_Changed(object? sender, EventArgs e)
    {
        try { if (sender is ClientCrawler c) await _builder.UpdateFacingTileAsync(c); }
        catch (Exception ex) { Console.Error.WriteLine($"Error updating facing tile: {ex.Message}"); }
    }

    private static async Task<Dto.Crawler> CreateCrawler(HttpClient http, Guid appKey, Dto.Settings? settings = null)
    {
        var response = await http.PostAsJsonAsync($"/crawlers?appKey={appKey}", settings);
        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Dto.Crawler>()
            is Dto.Crawler dto ? dto : throw new FormatException("Failed to read a crawler");
    }

    private class RemoteContestLabyrinthBuilder(ICrawler first) : IBuilder
    {
        private readonly object _tileLock = new();
        public readonly int XStart = first.X, YStart = first.Y;
        public readonly int Width = first.Y * 2 + 3, Height = first.Y * 2 + 1;
        public readonly Tile[,] Tiles = new Tile[first.Y * 2 + 3, first.Y * 2 + 1];

        public event EventHandler<StartEventArgs>? StartPositionFound;

        public Tile[,] Build()
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                if (int.IsOddInteger(x + y)) Tiles[x, y] = new Unknown();
                else if (int.IsEvenInteger(Math.Min(Math.Min(x, Width + 1 - x), Math.Min(y, Height + 1 - y))))
                    Tiles[x, y] = Wall.Singleton;
                else Tiles[x, y] = new Room();
            }

            StartPositionFound?.Invoke(this, new StartEventArgs(XStart, Height / 2));
            return Tiles;
        }

        private bool InRange(int val, int max) => 0 <= val && val < max;

        public void UpdateFacingTile(ClientCrawler crawler)
        {
            var (x, y) = (crawler.X + crawler.Direction.DeltaX, crawler.Y + crawler.Direction.DeltaY);
            lock (_tileLock)
            {
                if (!InRange(x, Width) || !InRange(y, Height) || Tiles[x, y] is not Unknown) return;
                var task = crawler.FacingTileType;
                if (task.IsCompleted)
                    Tiles[x, y] = task.Result.NewTile();
                else
                    task.ContinueWith(t => { lock (_tileLock) { if (Tiles[x, y] is Unknown) Tiles[x, y] = t.Result.NewTile(); } },
                        TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public async Task UpdateFacingTileAsync(ClientCrawler crawler)
        {
            var (x, y) = (crawler.X + crawler.Direction.DeltaX, crawler.Y + crawler.Direction.DeltaY);
            var tileType = await crawler.FacingTileType;
            lock (_tileLock)
            {
                if (InRange(x, Width) && InRange(y, Height) && Tiles[x, y] is Unknown)
                    Tiles[x, y] = tileType.NewTile();
            }
        }
    }
}
