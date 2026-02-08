using Labyrinth.Crawl;
using Labyrinth.Items;

namespace Labyrinth.Exploration;

public interface IExplorer
{
    ICrawler Crawler { get; }
    ExplorationGoal CurrentGoal { get; }
    Task<int> Explore(int maxSteps, Inventory? bag = null);
    Task<bool> Step(Inventory bag);
    event EventHandler<CrawlingEventArgs>? PositionChanged;
    event EventHandler<CrawlingEventArgs>? DirectionChanged;
}
