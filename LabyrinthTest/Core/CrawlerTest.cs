namespace LabyrinthTest.Core;

using global::Labyrinth.Build;
using global::Labyrinth.Core;

[TestFixture]
public class CrawlerTest
{
    private static Labyrinth.Labyrinth CreateMaze(string asciiMap) =>
        new(new AsciiParser(asciiMap));

    [Test]
    public async Task GetFacingTileType_FacingWall_ReturnsWall()
    {
        var maze = CreateMaze("""
            +---+
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 1, Direction.North);

        var result = await crawler.GetFacingTileTypeAsync();

        Assert.That(result, Is.EqualTo(TileType.Wall));
    }

    [Test]
    public async Task TryWalk_FacingRoom_MovesForward()
    {
        var maze = CreateMaze("""
            +---+
            |   |
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 2, Direction.North);

        var result = await crawler.TryWalkAsync();

        Assert.That(result, Is.True);
        Assert.That(crawler.Y, Is.EqualTo(1));
    }

    [Test]
    public async Task TryWalk_FacingWall_StaysInPlace()
    {
        var maze = CreateMaze("""
            +---+
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 1, Direction.North);

        var result = await crawler.TryWalkAsync();

        Assert.That(result, Is.False);
        Assert.That(crawler.Y, Is.EqualTo(1));
    }

    [Test]
    public void TurnLeft_FacingNorth_FacesWest()
    {
        var maze = CreateMaze("""
            +---+
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 1, Direction.North);

        crawler.TurnLeft();

        Assert.That(crawler.Direction, Is.EqualTo(Direction.West));
    }

    [Test]
    public void TurnRight_FacingNorth_FacesEast()
    {
        var maze = CreateMaze("""
            +---+
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 1, Direction.North);

        crawler.TurnRight();

        Assert.That(crawler.Direction, Is.EqualTo(Direction.East));
    }

    [Test]
    public async Task GetFacingTileType_FacingClosedDoor_ReturnsDoor()
    {
        var maze = CreateMaze("""
            +---+
            |k /|
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 2, Direction.North);

        var result = await crawler.GetFacingTileTypeAsync();

        Assert.That(result, Is.EqualTo(TileType.Room));
    }

    [Test]
    public async Task TryWalk_WithCancellation_ThrowsOperationCanceledException()
    {
        var maze = CreateMaze("""
            +---+
            | x |
            +---+
            """);
        var crawler = new Crawler(maze, 2, 1, Direction.North);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await crawler.TryWalkAsync(cts.Token));
    }
}
