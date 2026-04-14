using Backend.Services;
using Backend.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddSingleton<MatchStore>();

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors();

app.MapGet("/api/test", () => "test");

app.MapPost("/api/match", (MatchRequestDto request, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.GameName))
        return Results.BadRequest("GameName is required.");
    var match = store.CreateMatch(request);

    var url = $"/api/match/{match.Id}";
    
    return Results.Created(url, new MatchResponseDto
    {
        Id = match.Id, 
        Url = url
    });
});

app.MapGet("/api/match/{id}", (string id, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Match ID is required.");
    if (store.TryGetMatch(id, out var match))
        return Results.Ok(match);
    return Results.NotFound();
});

//TODOS
// PUT /api/match/{id}/player/{playerId}/score – Uppdatera poäng
// PUT /api/match/{id}/player/{playerId}/name – Byt namn på spelare
// POST /api/match/{id}/player – Lägg till spelare
// POST /api/match/{id}/reset – Starta om matchen (nollställ poäng)
// POST /api/match/{id}/clone – Skapa en ny match med samma inställningar
// DELETE /api/match/{id}/player/{playerId} – Ta bort spelare

app.MapFallbackToFile("index.html");

app.Run();
