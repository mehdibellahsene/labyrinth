using Labyrinth.Crawl;
using Labyrinth.Exploration;
using Labyrinth.Tiles;

namespace LabyrinthTest.Exploration;

public class PathfinderTest
{
    private SharedMap _map = null!;
    private Pathfinder _pathfinder = null!;
    private Guid _crawlerId;

    [SetUp]
    public void Setup()
    {
        _map = new SharedMap();
        _pathfinder = new Pathfinder(_map);
        _crawlerId = Guid.NewGuid();
    }

    [Test]
    public void FindPath_ReturnsFailureForUnknownMap()
    {
        var result = _pathfinder.FindPath(0, 0, 5, 5);

        // With no known tiles, can't find a path to distant target
        Assert.That(result.Found, Is.False);
    }

    [Test]
    public void FindPath_FindsDirectPath()
    {
        // Create a straight corridor
        for (int x = 0; x <= 5; x++)
        {
            _map.UpdateTile(x, 0, typeof(Room), _crawlerId);
        }

        var result = _pathfinder.FindPath(0, 0, 5, 0);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Distance, Is.EqualTo(5));
        Assert.That(result.Steps.All(d => d == Direction.East), Is.True);
    }

    [Test]
    public void FindPath_FindsPathAroundWall()
    {
        // Create L-shaped path around wall
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Wall), _crawlerId);
        _map.UpdateTile(0, 1, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 1, typeof(Room), _crawlerId);

        var result = _pathfinder.FindPath(0, 0, 1, 1);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Distance, Is.EqualTo(2));
    }

    [Test]
    public void FindPath_ReturnsFailureWhenBlocked()
    {
        // Completely surrounded by walls
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Wall), _crawlerId);
        _map.UpdateTile(-1, 0, typeof(Wall), _crawlerId);
        _map.UpdateTile(0, 1, typeof(Wall), _crawlerId);
        _map.UpdateTile(0, -1, typeof(Wall), _crawlerId);

        var result = _pathfinder.FindPath(0, 0, 5, 5);

        Assert.That(result.Found, Is.False);
    }

    [Test]
    public void FindPath_ThroughDoorWithKey()
    {
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Door), _crawlerId);
        _map.UpdateTile(2, 0, typeof(Room), _crawlerId);

        var result = _pathfinder.FindPath(0, 0, 2, 0, hasKey: true);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Distance, Is.EqualTo(2));
    }

    [Test]
    public void FindPath_BlockedByDoorWithoutKey()
    {
        // Fully enclosed area - no unknown tiles to escape through
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Door), _crawlerId);
        _map.UpdateTile(2, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(0, 1, typeof(Wall), _crawlerId);
        _map.UpdateTile(0, -1, typeof(Wall), _crawlerId);
        _map.UpdateTile(-1, 0, typeof(Wall), _crawlerId);
        _map.UpdateTile(-1, -1, typeof(Wall), _crawlerId);
        _map.UpdateTile(-1, 1, typeof(Wall), _crawlerId);
        _map.UpdateTile(1, -1, typeof(Wall), _crawlerId);
        _map.UpdateTile(1, 1, typeof(Wall), _crawlerId);
        _map.UpdateTile(2, -1, typeof(Wall), _crawlerId);
        _map.UpdateTile(2, 1, typeof(Wall), _crawlerId);
        _map.UpdateTile(3, 0, typeof(Wall), _crawlerId);
        _map.UpdateTile(3, -1, typeof(Wall), _crawlerId);
        _map.UpdateTile(3, 1, typeof(Wall), _crawlerId);

        var result = _pathfinder.FindPath(0, 0, 2, 0, hasKey: false);

        Assert.That(result.Found, Is.False);
    }

    [Test]
    public void FindPath_ThroughOpenDoor()
    {
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Door), _crawlerId);
        _map.MarkDoorOpened(1, 0);
        _map.UpdateTile(2, 0, typeof(Room), _crawlerId);

        var result = _pathfinder.FindPath(0, 0, 2, 0, hasKey: false);

        Assert.That(result.Found, Is.True);
    }

    [Test]
    public void FindNearestUnknown_FindsAdjacentUnknown()
    {
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);

        var result = _pathfinder.FindNearestUnknown(0, 0);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Distance, Is.EqualTo(1));
    }

    [Test]
    public void FindNearestKey_FindsKey()
    {
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(2, 0, typeof(Room), _crawlerId);
        _map.MarkKeyFound(2, 0);

        var result = _pathfinder.FindNearestKey(0, 0);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Target, Is.EqualTo((2, 0)));
    }

    [Test]
    public void FindNearestDoor_FindsClosedDoor()
    {
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(2, 0, typeof(Door), _crawlerId);

        var result = _pathfinder.FindNearestDoor(0, 0, hasKey: true);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Target, Is.EqualTo((2, 0)));
    }

    [Test]
    public void FindExit_FindsOutside()
    {
        _map.UpdateTile(0, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(1, 0, typeof(Room), _crawlerId);
        _map.UpdateTile(2, 0, typeof(Outside), _crawlerId);

        var result = _pathfinder.FindExit(0, 0);

        Assert.That(result.Found, Is.True);
        Assert.That(result.Target, Is.EqualTo((2, 0)));
    }
}
