using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace Labyrinth.Exploration;

public class SmartExplorer : IExplorer
{
    private readonly ICrawler _crawler;
    private readonly ISharedMap _map;
    private readonly Pathfinder _pathfinder;
    private readonly Guid _explorerId = Guid.NewGuid();
    private readonly HashSet<(int, int)> _scannedPositions = new();
    private readonly Random _random = new();
    private List<Direction>? _currentPath;
    private int _pathIndex;

    public ICrawler Crawler => _crawler;
    public ExplorationGoal CurrentGoal { get; private set; } = ExplorationGoal.Explore;

    public SmartExplorer(ICrawler crawler, ISharedMap map)
    {
        _crawler = crawler;
        _map = map;
        _pathfinder = new Pathfinder(map);
        _map.UpdateTile(crawler.X, crawler.Y, typeof(Room), _explorerId);
    }

    public event EventHandler<CrawlingEventArgs>? PositionChanged;
    public event EventHandler<CrawlingEventArgs>? DirectionChanged;

    public async Task<int> Explore(int maxSteps, Inventory? bag = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxSteps, 0, nameof(maxSteps));
        bag ??= new MyInventory();
        int steps = maxSteps;

        while (steps > 0 && await _crawler.FacingTileType != typeof(Outside))
        {
            if (!await Step(bag)) break;
            steps--;
        }
        return steps;
    }

    public async Task<bool> Step(Inventory bag)
    {
        await ScanSurroundings();

        if (_currentPath != null && _pathIndex < _currentPath.Count)
        {
            var nextDir = _currentPath[_pathIndex];
            if (_map.IsTraversable(_crawler.X + nextDir.DeltaX, _crawler.Y + nextDir.DeltaY, bag.HasItems))
                return await FollowPath(bag);
            _currentPath = null;
        }

        var target = DetermineTarget(bag.HasItems);
        CurrentGoal = target.Goal;

        if (target.Goal == ExplorationGoal.Wander || target.Path is not { Found: true })
            return await WanderStep(bag);

        _currentPath = target.Path.Steps.ToList();
        _pathIndex = 0;
        return await FollowPath(bag);
    }

    private async Task ScanSurroundings()
    {
        if (!_scannedPositions.Add((_crawler.X, _crawler.Y))) return;

        _map.UpdateTile(_crawler.X, _crawler.Y, typeof(Room), _explorerId);

        for (int i = 0; i < 4; i++)
        {
            var facingType = await _crawler.FacingTileType;
            _map.UpdateTile(_crawler.X + _crawler.Direction.DeltaX, _crawler.Y + _crawler.Direction.DeltaY, facingType, _explorerId);
            _crawler.Direction.TurnLeft();
            var ft = await _crawler.FacingTileType;
            DirectionChanged?.Invoke(this, new CrawlingEventArgs(_crawler.X, _crawler.Y, _crawler.Direction, ft));
        }
    }

    private async Task<bool> FollowPath(Inventory bag)
    {
        if (_currentPath == null || _pathIndex >= _currentPath.Count) return true;

        var targetDir = _currentPath[_pathIndex];
        while (_crawler.Direction.DeltaX != targetDir.DeltaX || _crawler.Direction.DeltaY != targetDir.DeltaY)
        {
            _crawler.Direction.TurnLeft();
            var ft = await _crawler.FacingTileType;
            DirectionChanged?.Invoke(this, new CrawlingEventArgs(_crawler.X, _crawler.Y, _crawler.Direction, ft));
        }

        var (wx, wy) = (_crawler.X + targetDir.DeltaX, _crawler.Y + targetDir.DeltaY);
        var targetTile = _map.GetTile(wx, wy);
        bool wasDoor = targetTile is { IsDoor: true, IsDoorOpen: false };

        if (await _crawler.TryWalk(bag) is { } result)
        {
            if (wasDoor) _map.MarkDoorOpened(wx, wy);
            await CollectItems(bag, result);
            PositionChanged?.Invoke(this, new CrawlingEventArgs(_crawler.X, _crawler.Y, _crawler.Direction, await _crawler.FacingTileType));
            _pathIndex++;
            return true;
        }

        _currentPath = null;
        return true;
    }

    private async Task<bool> WanderStep(Inventory bag)
    {
        int offset = _random.Next(4);
        for (int j = 0; j < offset; j++)
        {
            _crawler.Direction.TurnLeft();
            DirectionChanged?.Invoke(this, new CrawlingEventArgs(_crawler.X, _crawler.Y, _crawler.Direction, await _crawler.FacingTileType));
        }

        for (int i = 0; i < 4; i++)
        {
            var facingType = await _crawler.FacingTileType;
            if (facingType == typeof(Outside)) return false;

            if (facingType != typeof(Wall) && await _crawler.TryWalk(bag) is { } result)
            {
                await CollectItems(bag, result);
                PositionChanged?.Invoke(this, new CrawlingEventArgs(_crawler.X, _crawler.Y, _crawler.Direction, await _crawler.FacingTileType));
                return true;
            }

            _crawler.Direction.TurnLeft();
            DirectionChanged?.Invoke(this, new CrawlingEventArgs(_crawler.X, _crawler.Y, _crawler.Direction, await _crawler.FacingTileType));
        }
        return true;
    }

    private ExplorationTarget DetermineTarget(bool hasKey)
    {
        var (x, y) = (_crawler.X, _crawler.Y);

        var exitPath = _pathfinder.FindExit(x, y, hasKey);
        if (exitPath.Found) return ExplorationTarget.Create(ExplorationGoal.Exit, exitPath, 0);

        if (hasKey)
        {
            var doorPath = _pathfinder.FindNearestDoor(x, y, true);
            if (doorPath.Found) return ExplorationTarget.Create(ExplorationGoal.OpenDoor, doorPath, 1);
        }

        if (_map.GetClosedDoors().Any())
        {
            var keyPath = _pathfinder.FindNearestKey(x, y);
            if (keyPath.Found) return ExplorationTarget.Create(ExplorationGoal.CollectKey, keyPath, 2);
        }

        var unknownPath = _pathfinder.FindNearestUnknown(x, y, hasKey);
        if (unknownPath.Found) return ExplorationTarget.Create(ExplorationGoal.Explore, unknownPath, 3);

        return ExplorationTarget.None;
    }

    private async Task CollectItems(Inventory bag, Inventory roomInventory)
    {
        if (!roomInventory.HasItems) return;
        await bag.TryMoveItemsFrom(roomInventory, roomInventory.ItemTypes.Select(_ => true).ToList());
        if (bag.ItemTypes.Any(t => t == typeof(Key)))
            _map.MarkKeyCollected(_crawler.X, _crawler.Y);
    }
}
