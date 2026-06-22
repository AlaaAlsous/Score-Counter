using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.SignalR;
using Shared;

public static class MatchEndpoints
{
    public static void MapMatchEndpoints(this WebApplication app)
    {
        app.MapGet("/api/match/{id}", (string id, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("Match ID is required.");
            if (store.TryGetMatch(id, out var match))
                return Results.Ok(match);
            return Results.NotFound("Match not found.");
        });

        app.MapPost("/api/match", (MatchRequestDto request, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.GameName))
                return Results.BadRequest("GameName is required.");
            var match = store.CreateMatch(request);
            var url = $"/match/{match.Id}";
            return Results.Created(url, new MatchResponseDto { Id = match.Id, Url = url });
        });

        app.MapPost("/api/match/{id}/reset", async (string id, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("Match ID is required.");
            if (!store.TryGetMatch(id, out var match))
                return Results.NotFound("Match not found.");

            var resetRequest = new MatchRequestDto
            {
                GameName = match!.GameName,
                HighScoreWins = match.HighScoreWins,
                MaxPlayers = match.MaxPlayers,
                PlayersLocked = match.PlayersLocked,
                StartScore = match.StartScore,
                MaxScore = match.MaxScore,
                PlayerNames = match.OriginalPlayerNames.ToList()
            };
            var newMatch = store.CreateMatch(resetRequest);
            var url = $"/match/{newMatch.Id}";
            await hub.Clients.Group(id).SendAsync("MatchReset", url);
            return Results.Created(url, new MatchResponseDto { Id = newMatch.Id, Url = url });
        });

        app.MapPost("/api/match/{id}/clone", (string id, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("Match ID is required.");
            if (!store.TryGetMatch(id, out var match))
                return Results.NotFound("Match not found.");

            var cloneRequest = new MatchRequestDto
            {
                GameName = match!.GameName,
                HighScoreWins = match.HighScoreWins,
                MaxPlayers = match.MaxPlayers,
                PlayersLocked = match.PlayersLocked,
                StartScore = match.StartScore,
                MaxScore = match.MaxScore,
                PlayerNames = match.Players.Select(p => p.Name).ToList()
            };
            var clonedMatch = store.CreateMatch(cloneRequest);

            foreach (var originalPlayer in match.Players)
            {
                var clonedPlayer = clonedMatch.Players.FirstOrDefault(p => p.Name == originalPlayer.Name);
                if (clonedPlayer is not null && clonedPlayer.Score != originalPlayer.Score)
                {
                    store.UpdatePlayerScore(clonedMatch.Id, clonedPlayer.Id, originalPlayer.Score, null, null, out _, out _);
                }
            }

            var url = $"/match/{clonedMatch.Id}";
            return Results.Created(url, new MatchResponseDto { Id = clonedMatch.Id, Url = url });
        });

        app.MapPost("/api/match/{id}/player", async (string id, string playerName, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Match ID is required.");
            if (string.IsNullOrWhiteSpace(playerName)) return Results.BadRequest("Player name is required.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Match not found.");
            if (match!.IsFinished) return Results.BadRequest("Match is finished and cannot be modified.");
            if (match.PlayersLocked) return Results.BadRequest("Players are locked for this match.");
            if (match.MaxPlayers > 0 && match.Players.Count >= match.MaxPlayers) return Results.BadRequest("Match is full.");
            if (match.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                return Results.BadRequest("Player name already exists in this match.");

            store.AddPlayer(id, playerName, out var newPlayer);
            await hub.Clients.Group(id).SendAsync("PlayerAdded", newPlayer);
            return Results.Ok(newPlayer);
        });

        app.MapPut("/api/match/{id}/player/{playerId}/score", async (string id, string playerId, int? newScore, int? amount, string? operation, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Match ID is required.");
            if (string.IsNullOrWhiteSpace(playerId)) return Results.BadRequest("Player ID is required.");
            if (!Guid.TryParse(playerId, out var playerGuid)) return Results.BadRequest("Invalid player ID.");
            if (!newScore.HasValue && (!amount.HasValue || string.IsNullOrWhiteSpace(operation)))
                return Results.BadRequest("Either newScore or amount+operation is required.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Match not found.");
            if (match!.IsFinished) return Results.BadRequest("Match is finished and cannot be modified.");

            if (!store.UpdatePlayerScore(id, playerGuid, newScore, amount, operation, out var updated, out var entry))
                return Results.NotFound("Player not found in match.");
            await hub.Clients.Group(id).SendAsync("ScoreChanged", playerGuid, updated!.Score);
            if (entry is not null)
                await hub.Clients.Group(id).SendAsync("ScoreHistoryEntry", entry);
            return Results.Ok(updated);
        });

        app.MapPut("/api/match/{id}/player/{playerId}/name", async (string id, string playerId, string newName, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Match ID is required.");
            if (string.IsNullOrWhiteSpace(playerId)) return Results.BadRequest("Player ID is required.");
            if (string.IsNullOrWhiteSpace(newName)) return Results.BadRequest("New name is required.");
            if (!store.CheckNameForUniqueness(id, newName, playerId)) return Results.BadRequest("Name is already taken.");
            if (!Guid.TryParse(playerId, out var playerGuid)) return Results.BadRequest("Invalid player ID.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Match not found.");
            if (match!.IsFinished) return Results.BadRequest("Match is finished and cannot be modified.");

            if (!store.UpdatePlayerName(id, playerGuid, newName, out var updated))
                return Results.NotFound("Player not found or name already taken.");
            var trimmedName = newName.Trim();
            await hub.Clients.Group(id).SendAsync("PlayerRenamed", playerGuid, trimmedName);
            await hub.Clients.Group(id).SendAsync("ScoreHistoryRenamed", playerGuid, trimmedName);
            return Results.Ok(updated);
        });

        app.MapDelete("/api/match/{id}/player/{playerId}", async (string id, string playerId, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Match ID is required.");
            if (string.IsNullOrWhiteSpace(playerId)) return Results.BadRequest("Player ID is required.");
            if (!Guid.TryParse(playerId, out var playerGuid)) return Results.BadRequest("Invalid player ID.");
            if (!store.TryGetMatch(id, out var match)) return Results.NotFound("Match not found.");
            if (match!.IsFinished) return Results.BadRequest("Match is finished and cannot be modified.");

            if (!store.RemovePlayer(id, playerGuid, out var removed, out var playerName))
                return Results.NotFound("Player not found in match.");
            await hub.Clients.Group(id).SendAsync("PlayerRemoved", playerId);
            await hub.Clients.Group(id).SendAsync("PlayerHistoryRemoved", playerName);
            return Results.Ok(removed);
        });

        app.MapGet("/api/match/{id}/history", (string id, MatchStore store) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("Match ID is required.");
            if (!store.TryGetMatch(id, out _))
                return Results.NotFound("Match not found.");

            var history = store.GetScoreHistory(id);
            return Results.Ok(history);
        });

        app.MapPost("/api/match/{id}/finish", async (string id, MatchStore store, IHubContext<MatchEventHub> hub) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest("Match ID is required.");
            if (!store.FinishMatch(id, out var match)) return Results.NotFound("Match not found.");
            await hub.Clients.Group(id).SendAsync("MatchFinished");
            return Results.Ok(match);
        });
    }
}
