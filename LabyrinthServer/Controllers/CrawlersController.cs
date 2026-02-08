using ApiTypes;
using LabyrinthServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabyrinthServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CrawlersController(ILabyrinthService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetCrawlers([FromQuery] Guid appKey)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        return Ok(service.GetCrawlers(appKey));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCrawler([FromQuery] Guid appKey, [FromBody] Settings? settings = null)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        var crawler = await service.CreateCrawler(appKey, settings);
        if (crawler == null) return StatusCode(403, Problem("This app key reached its 3 instances of simultaneous crawlers", statusCode: 403));
        return CreatedAtAction(nameof(GetCrawler), new { id = crawler.Id, appKey }, crawler);
    }

    [HttpGet("{id}")]
    public IActionResult GetCrawler([FromQuery] Guid appKey, Guid id)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        return Ok(service.GetCrawler(appKey, id));
    }

    [HttpPatch("{id}")]
    public IActionResult UpdateCrawler([FromQuery] Guid appKey, Guid id, [FromBody] Crawler update)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        var (crawler, walkFailed) = service.UpdateCrawler(appKey, id, update);
        if (crawler == null) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (walkFailed) return Conflict(Problem("Cannot walk through this tile", statusCode: 409));
        return Ok(crawler);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCrawler([FromQuery] Guid appKey, Guid id)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        service.DeleteCrawler(appKey, id);
        return NoContent();
    }

    [HttpGet("{id}/bag")]
    public IActionResult GetBag([FromQuery] Guid appKey, Guid id)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        return Ok(service.GetBag(appKey, id));
    }

    [HttpPut("{id}/bag")]
    public IActionResult UpdateBag([FromQuery] Guid appKey, Guid id, [FromBody] IEnumerable<InventoryItem> items)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        var bag = service.UpdateBag(appKey, id, items);
        if (bag == null) return Conflict(Problem("Inventory operation failed", statusCode: 409));
        return Ok(bag);
    }

    [HttpGet("{id}/items")]
    public IActionResult GetItems([FromQuery] Guid appKey, Guid id)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        return Ok(service.GetItems(appKey, id));
    }

    [HttpPut("{id}/items")]
    public IActionResult UpdateItems([FromQuery] Guid appKey, Guid id, [FromBody] IEnumerable<InventoryItem> items)
    {
        if (appKey == Guid.Empty) return Unauthorized(Problem("A valid app key is required", statusCode: 401));
        if (!service.CrawlerExists(id)) return NotFound(Problem("Unknown crawler", statusCode: 404));
        if (!service.HasAccess(appKey, id)) return StatusCode(403, Problem("This app key cannot access this crawler", statusCode: 403));
        var result = service.UpdateItems(appKey, id, items);
        if (result == null) return Conflict(Problem("Failed to complete item transfer", statusCode: 409));
        return Ok(result);
    }
}
