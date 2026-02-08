using ApiTypes;
using LabyrinthServer.Models;

namespace LabyrinthServer.Services;

public interface ILabyrinthService
{
    IEnumerable<Crawler> GetCrawlers(Guid appKey);
    Task<Crawler?> CreateCrawler(Guid appKey, Settings? settings);
    Crawler? GetCrawler(Guid appKey, Guid crawlerId);
    (Crawler? crawler, bool walkFailed) UpdateCrawler(Guid appKey, Guid crawlerId, Crawler update);
    bool DeleteCrawler(Guid appKey, Guid crawlerId);
    IEnumerable<InventoryItem>? GetBag(Guid appKey, Guid crawlerId);
    IEnumerable<InventoryItem>? UpdateBag(Guid appKey, Guid crawlerId, IEnumerable<InventoryItem> items);
    IEnumerable<InventoryItem>? GetItems(Guid appKey, Guid crawlerId);
    IEnumerable<InventoryItem>? UpdateItems(Guid appKey, Guid crawlerId, IEnumerable<InventoryItem> items);
    bool HasAccess(Guid appKey, Guid crawlerId);
    bool CrawlerExists(Guid crawlerId);
}
