using Labyrinth;
using Labyrinth.Build;
using Labyrinth.Crawl;
using Labyrinth.Exploration;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace LabyrinthTest.Exploration;

public class SmartExplorerTest
{
    [Test]
    public async Task Explore_FindsExitInSimpleLabyrinth()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            + +
            |x|
            +-+
            """));
        var crawler = labyrinth.NewCrawler();
        var map = new SharedMap();
        var explorer = new SmartExplorer(crawler, map);

        var remaining = await explorer.Explore(100);

        Assert.That(remaining, Is.GreaterThan(0));
    }

    [Test]
    public async Task Explore_FindsExitWithDoorAndKey()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            +-/-+
            | k |
            | x |
            +---+
            """));
        var crawler = labyrinth.NewCrawler();
        var map = new SharedMap();
        var explorer = new SmartExplorer(crawler, map);

        var remaining = await explorer.Explore(200);

        Assert.That(remaining, Is.GreaterThan(0));
    }

    [Test]
    public async Task Explore_FindsExitInConcentricMaze()
    {
        // Two-layer concentric maze with matched key/door pairs
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            +--+
            |kx|
            +/-+
            | k/
            +--+
            """));
        var crawler = labyrinth.NewCrawler();
        var map = new SharedMap();
        var explorer = new SmartExplorer(crawler, map);

        var remaining = await explorer.Explore(500);

        Assert.That(remaining, Is.GreaterThan(0));
    }

    [Test]
    public async Task Step_UpdatesMap()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            +-+
            | |
            |x|
            +-+
            """));
        var crawler = labyrinth.NewCrawler();
        var map = new SharedMap();
        var explorer = new SmartExplorer(crawler, map);
        var bag = new MyInventory();

        await explorer.Step(bag);

        // After a step, the map should have discovered tiles
        var allTiles = map.GetAllTiles();
        Assert.That(allTiles.Count, Is.GreaterThan(1));
    }

    [Test]
    public void Explore_ThrowsOnInvalidMaxSteps()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            + +
            |x|
            +-+
            """));
        var crawler = labyrinth.NewCrawler();
        var map = new SharedMap();
        var explorer = new SmartExplorer(crawler, map);

        Assert.That(
            () => explorer.Explore(0),
            Throws.TypeOf<ArgumentOutOfRangeException>()
        );

        Assert.That(
            () => explorer.Explore(-5),
            Throws.TypeOf<ArgumentOutOfRangeException>()
        );
    }
}
