using Backend.Services;
using Shared;

public static class MatchEndpoints
{
    public static void MapMatchEndpoints(this WebApplication app)
    {
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
            var url = $"/match/{match.Id}";
            return Results.Created(url, new MatchResponseDto { Id = match.Id, Url = url });
        });

        app.MapPost("/api/match/{id}/reset", (string id, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("Matchens ID krävs.");
            if (!store.TryGetMatch(id, out var match))
                return Results.NotFound("Matchen hittades inte.");

            var resetRequest = new MatchRequestDto
            {
                GameName = match!.GameName,
                HighScoreWins = match.HighScoreWins,
                MaxPlayers = match.MaxPlayers,
                PlayersLocked = match.PlayersLocked,
                StartScore = match.StartScore,
                PlayerNames = match.OriginalPlayerNames.ToList()
            };
            var newMatch = store.CreateMatch(resetRequest);
            var url = $"/match/{newMatch.Id}";
            return Results.Created(url, new MatchResponseDto { Id = newMatch.Id, Url = url });
        });

        app.MapPost("/api/match/{id}/clone", (string id, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("Matchen ID är required.");
            if (!store.TryGetMatch(id, out var match))
                return Results.NotFound("Matchen hittades inte.");

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
            var url = $"/match/{clonedMatch.Id}";
            return Results.Created(url, new MatchResponseDto { Id = clonedMatch.Id, Url = url });
        });

        app.MapPost("/api/match/{id}/player", (string id, string playerName, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Matchen ID är required.");
            if (string.IsNullOrWhiteSpace(playerName)) return Results.BadRequest("Spelarnamn är required.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Matchen hittades inte.");
            if (match!.IsFinished) return Results.BadRequest("Matchen är avslutad och kan inte ändras.");
            if (match.PlayersLocked) return Results.BadRequest("Spelare är låsta för denna match.");
            if (match.MaxPlayers > 0 && match.Players.Count >= match.MaxPlayers) return Results.BadRequest("Matchen är full.");
            if (match.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                return Results.BadRequest("Spelarnamnet finns redan i denna match.");

            store.AddPlayer(id, playerName, out var newPlayer);
            return Results.Ok(newPlayer);
        });

        app.MapPut("/api/match/{id}/player/{playerId}/score", (string id, string playerId, int? newScore, int? amount, string? operation, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Matchen ID är required.");
            if (string.IsNullOrWhiteSpace(playerId)) return Results.BadRequest("Spelarens ID är required.");
            if (!Guid.TryParse(playerId, out var playerGuid)) return Results.BadRequest("Ogiltigt spelar-ID.");
            if (!newScore.HasValue && (!amount.HasValue || string.IsNullOrWhiteSpace(operation)))
                return Results.BadRequest("Antingen newScore eller amount+operation krävs.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Matchen hittades inte.");
            if (match!.IsFinished) return Results.BadRequest("Matchen är avslutad och kan inte ändras.");

            if (!store.UpdatePlayerScore(id, playerGuid, newScore, amount, operation, out var updated))
                return Results.NotFound("Spelaren hittades inte i matchen.");
            return Results.Ok(updated);
        });

        app.MapPut("/api/match/{id}/player/{playerId}/name", (string id, string playerId, string newName, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Matchen ID är required.");
            if (string.IsNullOrWhiteSpace(playerId)) return Results.BadRequest("Spelarens ID är required.");
            if (string.IsNullOrWhiteSpace(newName)) return Results.BadRequest("Nytt namn är required.");
            if (!Guid.TryParse(playerId, out var playerGuid)) return Results.BadRequest("Ogiltigt spelar-ID.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Matchen hittades inte.");
            if (match!.IsFinished) return Results.BadRequest("Matchen är avslutad och kan inte ändras.");

            if (!store.UpdatePlayerName(id, playerGuid, newName, out var updated))
                return Results.NotFound("Spelaren hittades inte eller namnet är redan taget.");
            return Results.Ok(updated);
        });

        app.MapDelete("/api/match/{id}/player/{playerId}", (string id, string playerId, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Matchen ID är required.");
            if (string.IsNullOrWhiteSpace(playerId)) return Results.BadRequest("Spelarens ID är required.");
            if (!Guid.TryParse(playerId, out var playerGuid)) return Results.BadRequest("Ogiltigt spelar-ID.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Matchen hittades inte.");
            if (match!.IsFinished) return Results.BadRequest("Matchen är avslutad och kan inte ändras.");

            if (!store.RemovePlayer(id, playerGuid, out var removed))
                return Results.NotFound("Spelaren hittades inte i matchen.");
            return Results.Ok(removed);
        });

        app.MapPost("/api/match/{id}/finish", (string id, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Matchens ID krävs.");
            if (!store.FinishMatch(id, out var match)) return Results.NotFound("Matchen hittades inte.");
            return Results.Ok(match);
        });
    }
}
