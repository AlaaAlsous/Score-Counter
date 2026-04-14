using System.Collections.Concurrent;
using Backend.Models;
using Shared;

namespace Backend.Services;

public class MatchStore
{
    private readonly ConcurrentDictionary<string, GameMatch> _matches = new();

    public GameMatch CreateMatch(MatchRequestDto request)
    {
        var match = new GameMatch
        {
            GameName = request.GameName,
            HighScoreWins = request.HighScoreWins,
            MaxPlayers = request.MaxPlayers,
            PlayersLocked = request.PlayersLocked,
            StartScore = request.StartScore,
            Players = request.PlayerNames?.Where(n => !string.IsNullOrWhiteSpace(n))
                                        .Select(n => new GamePlayer { Name = n.Trim(), Score = request.StartScore })
                                        .ToList() ?? new List<GamePlayer>()
        };

        _matches[match.id] = match;
        return match;
    }
}