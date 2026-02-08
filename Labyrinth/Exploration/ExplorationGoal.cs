namespace Labyrinth.Exploration;

public enum ExplorationGoal { Exit, OpenDoor, CollectKey, Explore, Wander }

public class ExplorationTarget
{
    public ExplorationGoal Goal { get; init; }
    public int TargetX { get; init; }
    public int TargetY { get; init; }
    public PathResult? Path { get; init; }
    public int Priority { get; init; }

    public static ExplorationTarget Create(ExplorationGoal goal, PathResult path, int priority = 0) => new()
    {
        Goal = goal,
        TargetX = path.Target.X,
        TargetY = path.Target.Y,
        Path = path,
        Priority = priority
    };

    public static ExplorationTarget None => new() { Goal = ExplorationGoal.Wander, Priority = int.MaxValue };
}
