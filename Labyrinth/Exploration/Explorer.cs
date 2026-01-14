namespace Labyrinth.Exploration;

using global::Labyrinth.Core;

public class Explorer
{
    private readonly ICrawler _crawler;
    private readonly IExplorationStrategy _strategy;

    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    public event EventHandler<DirectionChangedEventArgs>? DirectionChanged;

    public Explorer(ICrawler crawler, IExplorationStrategy strategy)
    {
        _crawler = crawler;
        _strategy = strategy;
    }

    public async Task<int> GetOutAsync(int maxMoves, CancellationToken ct = default)
    {
        var remaining = maxMoves;

        while (remaining > 0)
        {
            ct.ThrowIfCancellationRequested();

            var action = await _strategy.GetNextActionAsync(_crawler, ct);
            remaining--;

            switch (action)
            {
                case Actions.TurnLeft:
                    _crawler.TurnLeft();
                    OnDirectionChanged();
                    break;
                case Actions.TurnRight:
                    _crawler.TurnRight();
                    OnDirectionChanged();
                    break;
                case Actions.Walk:
                    if (await _crawler.TryWalkAsync(ct))
                        OnPositionChanged();
                    break;
            }
        }

        return remaining;
    }

    private void OnPositionChanged() =>
        PositionChanged?.Invoke(this, new(_crawler.X, _crawler.Y, _crawler.Direction));

    private void OnDirectionChanged() =>
        DirectionChanged?.Invoke(this, new(_crawler.X, _crawler.Y, _crawler.Direction));
}

public record PositionChangedEventArgs(int X, int Y, Direction Direction);
public record DirectionChangedEventArgs(int X, int Y, Direction Direction);
