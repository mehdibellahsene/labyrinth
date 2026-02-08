using Labyrinth;
using Labyrinth.ApiClient;
using Labyrinth.Build;
using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Exploration;
using Labyrinth.Rendering;
using Labyrinth.Sys;
using Dto=ApiTypes;
using System.Text.Json;

const int OffsetY = 3;

void PrintUsage()
{
    Console.WriteLine("Labyrinth Explorer - Console Client");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  Labyrinth                                    Run with local labyrinth (demo mode)");
    Console.WriteLine("  Labyrinth <server-url> <appKey>              Connect to server with smart explorer");
    Console.WriteLine("  Labyrinth <server-url> <appKey> --random     Connect to server with random explorer");
    Console.WriteLine("  Labyrinth <server-url> <appKey> --multi <n>  Use n crawlers (1-3) with coordination");
    Console.WriteLine("  Labyrinth <server-url> <appKey> [settings.json]");
    Console.WriteLine();
}

Labyrinth.Labyrinth labyrinth;
ICrawler crawler;
Inventory? bag = null;
ContestSession? contest = null;
bool useSmartExplorer = true;
int crawlerCount = 1;

if (args.Length < 2)
{
    PrintUsage();
    Console.WriteLine("Running in local demo mode...");
    Console.WriteLine();

    labyrinth = new Labyrinth.Labyrinth(new AsciiParser("""
        +--+--------+
        |  /        |
        |  +--+--+  |
        |     |k    |
        +--+  |  +--+
           |k  x    |
        +  +-------/|
        |           |
        +-----------+
        """));
    crawler = labyrinth.NewCrawler();
}
else
{
    Dto.Settings? settings = null;

    for (int i = 2; i < args.Length; i++)
    {
        if (args[i] == "--random")
        {
            useSmartExplorer = false;
        }
        else if (args[i] == "--multi" && i + 1 < args.Length)
        {
            crawlerCount = Math.Clamp(int.Parse(args[++i]), 1, 3);
        }
        else if (args[i].EndsWith(".json"))
        {
            settings = JsonSerializer.Deserialize<Dto.Settings>(File.ReadAllText(args[i]));
        }
    }

    Console.WriteLine($"Connecting to {args[0]}...");
    contest = await ContestSession.Open(new Uri(args[0]), Guid.Parse(args[1]), settings);
    labyrinth = new (contest.Builder);
    crawler = await contest.NewCrawler();
    bag = contest.Bags.First();

    Console.WriteLine($"Connected! Crawler at ({crawler.X}, {crawler.Y})");
}

Console.Clear();
Console.SetCursorPosition(0, OffsetY);
Console.WriteLine(labyrinth);

bool visualDelay = args.Contains("--visual");

if (crawlerCount > 1 && contest != null)
{
    var coordinator = new CoordinatedExplorer(contest.SharedMap);
    var renderer = new MultiCrawlerRenderer(OffsetY, visualDelay, coordinator);

    var firstExplorer = coordinator.AddCrawler(crawler, bag!);
    renderer.RegisterCrawler(firstExplorer, bag!);

    for (int i = 1; i < crawlerCount; i++)
    {
        var newCrawler = await contest.NewCrawler();
        var newBag = contest.Bags.Skip(i).First();
        var newExplorer = coordinator.AddCrawler(newCrawler, newBag);
        renderer.RegisterCrawler(newExplorer, newBag);
    }

    await coordinator.ExploreCoordinated(3000);

    var stats = coordinator.GetStats();
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"Exploration complete! Discovered {stats.TotalTilesDiscovered} tiles, {stats.DoorsOpened}/{stats.DoorsDiscovered} doors opened");
}
else if (useSmartExplorer && contest != null)
{
    var renderer = new MultiCrawlerRenderer(OffsetY, visualDelay);
    var smartExplorer = new SmartExplorer(crawler, contest.SharedMap);
    renderer.RegisterCrawler(smartExplorer, bag!);
    await smartExplorer.Explore(3000, bag);
}
else
{
    var renderer = new MultiCrawlerRenderer(OffsetY, visualDelay);
    var explorer = new RandExplorer(
        crawler,
        new BasicEnumRandomizer<RandExplorer.Actions>()
    );
    renderer.AttachTo(explorer);
    await explorer.GetOut(3000, bag);
}

if (contest is not null)
{
    await contest.Close();
}

Console.SetCursorPosition(0, Console.WindowHeight - 1);
Console.WriteLine("Exploration finished. Press any key to exit...");
Console.ReadKey();
