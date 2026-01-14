namespace Labyrinth.Exploration;

using Labyrinth.Core;

public class WallFollowerStrategy : IExplorationStrategy
{
    public async Task<Actions> GetNextActionAsync(ICrawler crawler, CancellationToken ct = default)
    {
        var frontType = await crawler.GetFacingTileTypeAsync(ct);

        if (frontType != TileType.Wall)
            return Actions.Walk;

        return Actions.TurnLeft;
    }

    public void Reset() { }
}
