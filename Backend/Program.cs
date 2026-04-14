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

app.MapGet("/api/match/{id}", (string id, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");
    if (store.TryGetMatch(id, out var match))
        return Results.Ok(match);
    return Results.NotFound("Matchen hittades inte.");
});

app.MapPost("/api/match", (MatchRequestDto request, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.GameName))
        return Results.BadRequest("GameName är required.");
    var match = store.CreateMatch(request);

    var url = $"/api/match/{match.Id}";

    return Results.Created(url, new MatchResponseDto
    {
        Id = match.Id,
        Url = url
    });
});

app.MapPost("/api/match/{id}/reset", (string id, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");
    if (store.TryGetMatch(id, out var match))
    {
        lock (match!)
        {
            foreach (var player in match.Players)
            {
                player.Score = match.StartScore;
            }
        }
        store.UpdateMatch(id, match);
        return Results.Ok(match);
    }
    return Results.NotFound("Matchen hittades inte.");
});

app.MapPost("/api/match/{id}/player", (string id, string playerName, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");
    if (string.IsNullOrWhiteSpace(playerName))
        return Results.BadRequest("Spelarnamn är required.");

    if (store.TryGetMatch(id, out var match))
    {
        lock (match!)
        {
            if (match.PlayersLocked)
                return Results.BadRequest("Spelare är låsta för denna match.");
            if (match.Players.Count >= match.MaxPlayers)
                return Results.BadRequest("Matchen är full.");
            if (match.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                return Results.BadRequest("Spelarnamnet finns redan i denna match.");

            var newPlayer = new GamePlayer { Name = playerName.Trim(), Score = match.StartScore };
            match.Players.Add(newPlayer);
            store.UpdateMatch(id, match);
            return Results.Ok(newPlayer);
        }
    }
    return Results.NotFound("Matchen hittades inte.");
});

app.MapPost("/api/match/{id}/clone", (string id, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");

    if (store.TryGetMatch(id, out var match))
    {
        var cloneRequest = new MatchRequestDto
        {
            GameName = match!.GameName,
            HighScoreWins = match.HighScoreWins,
            MaxPlayers = match.MaxPlayers,
            PlayersLocked = match.PlayersLocked,
            StartScore = match.StartScore,
            PlayerNames = match.Players.Select(p => p.Name).ToList()
        };
        var clonedMatch = store.CreateMatch(cloneRequest);
        var url = $"/api/match/{clonedMatch.Id}";
        return Results.Created(url, new MatchResponseDto
        {
            Id = clonedMatch.Id,
            Url = url
        });
    }
    return Results.NotFound("Matchen hittades inte.");
});

app.MapPut("/api/match/{id}/player/{playerId}/score", (string id, string playerId, int newScore, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");
    if (string.IsNullOrWhiteSpace(playerId))
        return Results.BadRequest("Spelarens ID är required.");

    if (store.TryGetMatch(id, out var match))
    {
        lock (match!)
        {
            var player = match.Players.FirstOrDefault(p => p.Id.ToString() == playerId);
            if (player == null)
                return Results.NotFound("Spelaren hittades inte i matchen.");

            player.Score = newScore;
            store.UpdateMatch(id, match);
            return Results.Ok(player);
        }
    }
    return Results.NotFound("Matchen hittades inte.");
});

app.MapPut("/api/match/{id}/player/{playerId}/name", (string id, string playerId, string newName, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");
    if (string.IsNullOrWhiteSpace(playerId))
        return Results.BadRequest("Spelarens ID är required.");
    if (string.IsNullOrWhiteSpace(newName))
        return Results.BadRequest("Nytt namn är required.");

    if (store.TryGetMatch(id, out var match))
    {
        lock (match!)
        {
            var player = match.Players.FirstOrDefault(p => p.Id.ToString() == playerId);
            if (player == null)
                return Results.NotFound("Spelaren hittades inte i matchen.");

            if (match.Players.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                return Results.BadRequest("Spelarnamnet finns redan i denna match.");

            player.Name = newName.Trim();
            store.UpdateMatch(id, match);
            return Results.Ok(player);
        }
    }
    return Results.NotFound("Matchen hittades inte.");
});

app.MapDelete("/api/match/{id}/player/{playerId}", (string id, string playerId, MatchStore store) =>
{
    if (string.IsNullOrWhiteSpace(id))
        return Results.BadRequest("Matchen ID är required.");
    if (string.IsNullOrWhiteSpace(playerId))
        return Results.BadRequest("Spelarens ID är required.");

    if (store.TryGetMatch(id, out var match))
    {
        lock (match!)
        {
            var player = match.Players.FirstOrDefault(p => p.Id.ToString() == playerId);
            if (player == null)
                return Results.NotFound("Spelaren hittades inte i matchen.");

            match.Players.Remove(player);
            store.UpdateMatch(id, match);
            return Results.Ok(player);
        }
    }
    return Results.NotFound("Matchen hittades inte.");
});

app.MapFallbackToFile("index.html");

app.Run();
