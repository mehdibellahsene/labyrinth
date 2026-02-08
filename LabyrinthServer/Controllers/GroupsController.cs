using Microsoft.AspNetCore.Mvc;

namespace LabyrinthServer.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetGroups() => Ok(Array.Empty<GroupInfo>());
}

public record GroupInfo(string Name = "", int AppKeys = 0, int ActiveCrawlers = 0, int ApiCalls = 0);
