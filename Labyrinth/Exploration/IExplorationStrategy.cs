namespace Labyrinth.Exploration;

using global::Labyrinth.Core;

public interface IExplorationStrategy
{
    Task<Actions> GetNextActionAsync(ICrawler crawler, CancellationToken ct = default);
    void Reset();
}

public enum Actions { TurnLeft, TurnRight, Walk }
