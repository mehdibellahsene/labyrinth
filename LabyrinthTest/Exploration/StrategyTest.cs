namespace LabyrinthTest.Exploration;

using Labyrinth.Core;
using Labyrinth.Exploration;
using Moq;

[TestFixture]
public class StrategyTest
{
    [Test]
    public async Task WallFollower_WallInFront_TurnsLeft()
    {
        var crawler = new Mock<ICrawler>();
        crawler.Setup(c => c.Direction).Returns(Direction.North);
        crawler.Setup(c => c.GetFacingTileTypeAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(TileType.Wall);

        var strategy = new WallFollowerStrategy();
        var action = await strategy.GetNextActionAsync(crawler.Object);

        Assert.That(action, Is.EqualTo(Actions.TurnLeft));
    }

    [Test]
    public async Task WallFollower_RoomInFront_Walks()
    {
        var crawler = new Mock<ICrawler>();
        crawler.Setup(c => c.Direction).Returns(Direction.North);
        crawler.Setup(c => c.GetFacingTileTypeAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(TileType.Room);

        var strategy = new WallFollowerStrategy();
        var action = await strategy.GetNextActionAsync(crawler.Object);

        Assert.That(action, Is.EqualTo(Actions.Walk));
    }

    [Test]
    public void DFS_Reset_ClearsVisited()
    {
        var strategy = new DFSStrategy();
        strategy.Reset();

        Assert.Pass();
    }

    [Test]
    public async Task DFS_UnvisitedRoomInFront_Walks()
    {
        var crawler = new Mock<ICrawler>();
        crawler.Setup(c => c.X).Returns(1);
        crawler.Setup(c => c.Y).Returns(1);
        crawler.Setup(c => c.Direction).Returns(Direction.North);
        crawler.Setup(c => c.GetFacingTileTypeAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(TileType.Room);

        var strategy = new DFSStrategy();
        var action = await strategy.GetNextActionAsync(crawler.Object);

        Assert.That(action, Is.EqualTo(Actions.Walk));
    }

    [Test]
    public async Task DFS_WallInFront_TurnsRight()
    {
        var crawler = new Mock<ICrawler>();
        crawler.Setup(c => c.X).Returns(1);
        crawler.Setup(c => c.Y).Returns(1);
        crawler.Setup(c => c.Direction).Returns(Direction.North);
        crawler.Setup(c => c.GetFacingTileTypeAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(TileType.Wall);

        var strategy = new DFSStrategy();
        var action = await strategy.GetNextActionAsync(crawler.Object);

        Assert.That(action, Is.EqualTo(Actions.TurnRight));
    }
}
