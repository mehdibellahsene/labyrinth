using Labyrinth.Crawl;

namespace Labyrinth.Exploration;

public class PathResult
{
    public bool Found { get; init; }
    public IReadOnlyList<Direction> Steps { get; init; } = Array.Empty<Direction>();
    public int Distance => Steps.Count;
    public (int X, int Y) Target { get; init; }

    public static PathResult Success(List<Direction> steps, (int, int) target)
        => new() { Found = true, Steps = steps, Target = target };

    public static PathResult Failure((int, int) target)
        => new() { Found = false, Target = target };
}
