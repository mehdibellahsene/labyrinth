using LabyrinthServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILabyrinthService, LabyrinthService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var api = app.MapGroup("/api");

api.MapGet("/maze", (ILabyrinthService svc) => Results.Ok(svc.GetMaze()));

api.MapPost("/crawler", (ILabyrinthService svc) =>
{
    var id = svc.CreateCrawler();
    return Results.Created($"/api/crawler/{id}", new { id });
});

api.MapGet("/crawler/{id}/facing", (int id, ILabyrinthService svc) =>
{
    var tile = svc.GetFacingTile(id);
    return tile is null ? Results.NotFound() : Results.Ok(tile);
});

api.MapPost("/crawler/{id}/turn", (int id, bool left, ILabyrinthService svc) =>
    svc.TurnCrawler(id, left) ? Results.Ok() : Results.NotFound());

api.MapPost("/crawler/{id}/walk", (int id, ILabyrinthService svc) =>
    Results.Ok(svc.TryWalk(id)));

api.MapGet("/crawler/{id}/inventory", (int id, ILabyrinthService svc) =>
{
    var inv = svc.GetInventory(id);
    return inv is null ? Results.NotFound() : Results.Ok(inv);
});

app.Run();
