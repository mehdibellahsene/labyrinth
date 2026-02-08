using Labyrinth.Exploration;
using Labyrinth.Tiles;

namespace LabyrinthTest.Exploration;

public class SharedMapTest
{
    private SharedMap _map = null!;

    [SetUp]
    public void Setup()
    {
        _map = new SharedMap();
    }

    [Test]
    public void GetTile_ReturnsNullForUnknownPosition()
    {
        var tile = _map.GetTile(5, 5);
        Assert.That(tile, Is.Null);
    }

    [Test]
    public void UpdateTile_AddsTileToMap()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(3, 4, typeof(Room), crawlerId);

        var tile = _map.GetTile(3, 4);
        Assert.That(tile, Is.Not.Null);
        Assert.That(tile!.X, Is.EqualTo(3));
        Assert.That(tile.Y, Is.EqualTo(4));
        Assert.That(tile.TileType, Is.EqualTo(typeof(Room)));
        Assert.That(tile.DiscoveredBy, Is.EqualTo(crawlerId));
    }

    [Test]
    public void UpdateTile_DoesNotOverwriteKnownTile()
    {
        var crawler1 = Guid.NewGuid();
        var crawler2 = Guid.NewGuid();

        _map.UpdateTile(1, 1, typeof(Room), crawler1);
        _map.UpdateTile(1, 1, typeof(Wall), crawler2);

        var tile = _map.GetTile(1, 1);
        Assert.That(tile!.TileType, Is.EqualTo(typeof(Room)));
        Assert.That(tile.DiscoveredBy, Is.EqualTo(crawler1));
    }

    [Test]
    public void MarkKeyFound_SetsHasKeyTrue()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(2, 2, typeof(Room), crawlerId);
        _map.MarkKeyFound(2, 2);

        var tile = _map.GetTile(2, 2);
        Assert.That(tile!.HasKey, Is.True);
    }

    [Test]
    public void MarkKeyCollected_SetsHasKeyFalse()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(2, 2, typeof(Room), crawlerId);
        _map.MarkKeyFound(2, 2);
        _map.MarkKeyCollected(2, 2);

        var tile = _map.GetTile(2, 2);
        Assert.That(tile!.HasKey, Is.False);
    }

    [Test]
    public void MarkDoorOpened_SetsIsDoorOpenTrue()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(5, 5, typeof(Door), crawlerId);
        _map.MarkDoorOpened(5, 5);

        var tile = _map.GetTile(5, 5);
        Assert.That(tile!.IsDoorOpen, Is.True);
    }

    [Test]
    public void GetFrontierTiles_ReturnsAdjacentUnknownTiles()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(5, 5, typeof(Room), crawlerId);

        var frontier = _map.GetFrontierTiles().ToList();

        Assert.That(frontier, Has.Count.EqualTo(4));
        Assert.That(frontier, Contains.Item((5, 4)));
        Assert.That(frontier, Contains.Item((5, 6)));
        Assert.That(frontier, Contains.Item((4, 5)));
        Assert.That(frontier, Contains.Item((6, 5)));
    }

    [Test]
    public void GetFrontierTiles_ExcludesKnownTiles()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(5, 5, typeof(Room), crawlerId);
        _map.UpdateTile(5, 4, typeof(Wall), crawlerId);

        var frontier = _map.GetFrontierTiles().ToList();

        Assert.That(frontier, Has.Count.EqualTo(3));
        Assert.That(frontier, Does.Not.Contain((5, 4)));
    }

    [Test]
    public void GetKeyTiles_ReturnsOnlyTilesWithKeys()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Room), crawlerId);
        _map.UpdateTile(2, 2, typeof(Room), crawlerId);
        _map.MarkKeyFound(1, 1);

        var keyTiles = _map.GetKeyTiles().ToList();

        Assert.That(keyTiles, Has.Count.EqualTo(1));
        Assert.That(keyTiles[0].X, Is.EqualTo(1));
        Assert.That(keyTiles[0].Y, Is.EqualTo(1));
    }

    [Test]
    public void GetClosedDoors_ReturnsOnlyClosedDoors()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Door), crawlerId);
        _map.UpdateTile(2, 2, typeof(Door), crawlerId);
        _map.MarkDoorOpened(1, 1);

        var closedDoors = _map.GetClosedDoors().ToList();

        Assert.That(closedDoors, Has.Count.EqualTo(1));
        Assert.That(closedDoors[0].X, Is.EqualTo(2));
    }

    [Test]
    public void IsTraversable_ReturnsFalseForWall()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Wall), crawlerId);

        Assert.That(_map.IsTraversable(1, 1), Is.False);
    }

    [Test]
    public void IsTraversable_ReturnsTrueForRoom()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Room), crawlerId);

        Assert.That(_map.IsTraversable(1, 1), Is.True);
    }

    [Test]
    public void IsTraversable_ReturnsFalseForClosedDoorWithoutKey()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Door), crawlerId);

        Assert.That(_map.IsTraversable(1, 1, hasKey: false), Is.False);
    }

    [Test]
    public void IsTraversable_ReturnsTrueForClosedDoorWithKey()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Door), crawlerId);

        Assert.That(_map.IsTraversable(1, 1, hasKey: true), Is.True);
    }

    [Test]
    public void IsTraversable_ReturnsTrueForOpenDoor()
    {
        var crawlerId = Guid.NewGuid();
        _map.UpdateTile(1, 1, typeof(Door), crawlerId);
        _map.MarkDoorOpened(1, 1);

        Assert.That(_map.IsTraversable(1, 1, hasKey: false), Is.True);
    }

    [Test]
    public void ThreadSafety_ConcurrentUpdates()
    {
        var crawlerIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            var x = i;
            var crawlerId = crawlerIds[i % 10];
            tasks.Add(Task.Run(() => _map.UpdateTile(x, 0, typeof(Room), crawlerId)));
        }

        Task.WaitAll(tasks.ToArray());

        var allTiles = _map.GetAllTiles();
        Assert.That(allTiles.Count, Is.EqualTo(100));
    }

    [Test]
    public void ThreadSafety_ConcurrentReadsAndWrites()
    {
        var crawlerId = Guid.NewGuid();
        var tasks = new List<Task>();

        // Pre-populate some tiles
        for (int i = 0; i < 50; i++)
        {
            _map.UpdateTile(i, 0, typeof(Room), crawlerId);
        }

        // Concurrent writes
        for (int i = 50; i < 100; i++)
        {
            var x = i;
            tasks.Add(Task.Run(() => _map.UpdateTile(x, 0, typeof(Room), crawlerId)));
        }

        // Concurrent reads (GetFrontierTiles, GetAllTiles)
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() => _map.GetFrontierTiles().ToList()));
            tasks.Add(Task.Run(() => _map.GetAllTiles()));
            tasks.Add(Task.Run(() => _map.GetKeyTiles().ToList()));
            tasks.Add(Task.Run(() => _map.GetClosedDoors().ToList()));
        }

        // Concurrent key/door operations
        _map.UpdateTile(200, 0, typeof(Room), crawlerId);
        _map.UpdateTile(201, 0, typeof(Door), crawlerId);
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() => _map.MarkKeyFound(200, 0)));
            tasks.Add(Task.Run(() => _map.MarkKeyCollected(200, 0)));
            tasks.Add(Task.Run(() => _map.MarkDoorOpened(201, 0)));
            tasks.Add(Task.Run(() => _map.IsTraversable(200, 0)));
        }

        Assert.DoesNotThrow(() => Task.WaitAll(tasks.ToArray()));

        var allTiles = _map.GetAllTiles();
        Assert.That(allTiles.Count, Is.GreaterThanOrEqualTo(52));
    }
}
