namespace Labyrinth.Crawl
{
    public class CrawlingEventArgs(int x, int y, Direction dir, Type? facingTileType = null) : EventArgs
    {
        public CrawlingEventArgs(ICrawler crawler)
            : this(crawler.X, crawler.Y, crawler.Direction)
        {
        }
        public int X { get; } = x;
        public int Y { get; } = y;
        public Direction Direction { get; } = dir;
        public Type? FacingTileType { get; } = facingTileType;
    }
}
