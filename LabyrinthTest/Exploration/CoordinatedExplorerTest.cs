using Labyrinth;
using Labyrinth.Build;
using Labyrinth.Crawl;
using Labyrinth.Exploration;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace LabyrinthTest.Exploration;

public class CoordinatedExplorerTest
{
    [Test]
    public void AddCrawler_ReturnsSmartExplorer()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            + +
            |x|
            +-+
            """));
        var crawler = labyrinth.NewCrawler();
        var coordinator = new CoordinatedExplorer();

        var explorer = coordinator.AddCrawler(crawler);

        Assert.That(explorer, Is.Not.Null);
        Assert.That(explorer, Is.InstanceOf<SmartExplorer>());
        Assert.That(explorer.Crawler, Is.SameAs(crawler));
    }

    [Test]
    public async Task ExploreAll_FindsExit()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            +-/-+
            | k |
            | x |
            +---+
            """));

        var coordinator = new CoordinatedExplorer();
        var crawler = labyrinth.NewCrawler();
        var bag = new MyInventory();
        coordinator.AddCrawler(crawler, bag);

        var finder = await coordinator.ExploreAll(200);

        Assert.That(finder, Is.Not.Null);
    }

    [Test]
    public void GetStats_ReturnsValidStats()
    {
        var labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
            + +
            |x|
            +-+
            """));
        var coordinator = new CoordinatedExplorer();
        var crawler = labyrinth.NewCrawler();
        coordinator.AddCrawler(crawler);

        var stats = coordinator.GetStats();

        Assert.That(stats, Is.Not.Null);
        Assert.That(stats.CrawlerCount, Is.EqualTo(1));
        Assert.That(stats.TotalTilesDiscovered, Is.GreaterThanOrEqualTo(1));
    }
}
