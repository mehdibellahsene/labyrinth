using Labyrinth.ApiClient;
using Labyrinth.Crawl;
using Labyrinth.Exploration;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace Labyrinth.Rendering;

public class MultiCrawlerRenderer
{
    private static readonly Dictionary<Type, char> TileChars = new()
    {
        [typeof(Room)] = ' ', [typeof(Wall)] = '#', [typeof(Door)] = '/',
        [typeof(Unknown)] = '?', [typeof(Outside)] = 'O'
    };

    private static readonly ConsoleColor[] Colors = [ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Magenta];

    private readonly int _offsetY;
    private readonly bool _visualDelay;
    private readonly CoordinatedExplorer? _coordinator;
    private readonly object _consoleLock = new();
    private readonly List<CrawlerState> _crawlers = new();

    public MultiCrawlerRenderer(int offsetY, bool visualDelay, CoordinatedExplorer? coordinator = null)
    {
        _offsetY = offsetY;
        _visualDelay = visualDelay;
        _coordinator = coordinator;
    }

    public void RegisterCrawler(SmartExplorer explorer, Inventory bag)
    {
        var state = new CrawlerState
        {
            Index = _crawlers.Count, Color = Colors[_crawlers.Count % Colors.Length],
            Explorer = explorer, Bag = bag,
            PrevX = explorer.Crawler.X, PrevY = explorer.Crawler.Y + _offsetY
        };
        _crawlers.Add(state);
        explorer.PositionChanged += (_, e) => OnPositionChanged(state, e);
        explorer.DirectionChanged += (_, e) => OnDirectionChanged(state, e);
    }

    public void AttachTo(RandExplorer explorer)
    {
        var state = new CrawlerState
        {
            Index = 0, Color = Colors[0], RandExplorer = explorer,
            PrevX = explorer.Crawler.X, PrevY = explorer.Crawler.Y + _offsetY
        };
        _crawlers.Add(state);
        explorer.PositionChanged += (_, e) => OnPositionChanged(state, e);
        explorer.DirectionChanged += (_, e) => OnDirectionChanged(state, e);
    }

    private void OnPositionChanged(CrawlerState state, CrawlingEventArgs e)
    {
        lock (_consoleLock)
        {
            var occupant = _crawlers.FirstOrDefault(c => c != state && c.PrevX == state.PrevX && c.PrevY == state.PrevY);
            Console.SetCursorPosition(state.PrevX, state.PrevY);
            if (occupant != null)
            {
                Console.ForegroundColor = occupant.Color;
                Console.Write(DirToChar(occupant.LastDirection!));
                Console.ResetColor();
            }
            else Console.Write(' ');

            state.StepCount++;
            state.LastDirection = e.Direction;
            state.PrevX = e.X;
            state.PrevY = e.Y + _offsetY;
            DrawCrawlerChar(state, e);
            DrawHud();
        }
        if (_visualDelay) Thread.Sleep(50);
    }

    private void OnDirectionChanged(CrawlerState state, CrawlingEventArgs e)
    {
        lock (_consoleLock) { state.LastDirection = e.Direction; DrawCrawlerChar(state, e); }
    }

    private void DrawCrawlerChar(CrawlerState state, CrawlingEventArgs e)
    {
        if (e.FacingTileType is { } ft && ft != typeof(Outside) && TileChars.TryGetValue(ft, out var ch))
        {
            Console.SetCursorPosition(e.X + e.Direction.DeltaX, e.Y + e.Direction.DeltaY + _offsetY);
            Console.Write(ch);
        }
        Console.SetCursorPosition(e.X, e.Y + _offsetY);
        Console.ForegroundColor = state.Color;
        Console.Write(DirToChar(e.Direction));
        Console.ResetColor();
    }

    private void DrawHud()
    {
        Console.SetCursorPosition(0, 0);
        if (_coordinator != null)
        {
            var s = _coordinator.GetStats();
            Console.Write($"Tiles: {s.TotalTilesDiscovered}  Doors: {s.DoorsOpened}/{s.DoorsDiscovered}  Keys: {s.KeysFound}".PadRight(Console.WindowWidth - 1));
        }
        else
        {
            var s = _crawlers[0];
            Console.Write($"Goal: {s.Explorer?.CurrentGoal.ToString() ?? "Random"}  Steps: {s.StepCount}".PadRight(Console.WindowWidth - 1));
        }

        Console.SetCursorPosition(0, 1);
        if (_crawlers.Count > 1)
        {
            int cursor = 0;
            for (int i = 0; i < _crawlers.Count; i++)
            {
                if (i > 0) { Console.Write(" | "); cursor += 3; }
                var c = _crawlers[i];
                int keys = c.Bag?.ItemTypes.Count(t => t == typeof(Key)) ?? 0;
                var part = $"C{c.Index + 1}:{c.Explorer?.CurrentGoal.ToString() ?? "?"} S:{c.StepCount} K:{keys}";
                Console.ForegroundColor = c.Color;
                Console.Write(part);
                Console.ResetColor();
                cursor += part.Length;
            }
            if (cursor < Console.WindowWidth - 1) Console.Write(new string(' ', Console.WindowWidth - 1 - cursor));
        }
        else Console.Write(new string(' ', Console.WindowWidth - 1));
    }

    private static char DirToChar(Direction dir) =>
        "^<v>"[dir.DeltaX * dir.DeltaX + dir.DeltaX + dir.DeltaY + 1];

    private class CrawlerState
    {
        public int Index, PrevX, PrevY, StepCount;
        public ConsoleColor Color;
        public SmartExplorer? Explorer;
        public RandExplorer? RandExplorer;
        public Inventory? Bag;
        public Direction? LastDirection;
    }
}
