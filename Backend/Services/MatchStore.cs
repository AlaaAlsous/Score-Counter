using System.Collections.Concurrent;
//using Backend.Models;
using Shared;

namespace Backend.Services;

public class MatchStore
{
    private readonly ConcurrentDictionary<string, GameMatch> _matches = new();

    public GameMatch CreateMatch(MatchRequestDto request)
{
    var playerNames = request.PlayerNames?
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .Select(n => n.Trim())
        .ToList() ?? new List<string>();

    var match = new GameMatch
    {
        GameName = request.GameName,
        HighScoreWins = request.HighScoreWins,
        MaxPlayers = request.MaxPlayers,
        PlayersLocked = request.PlayersLocked,
        StartScore = request.StartScore,
        OriginalPlayerNames = playerNames.ToList(),
        Players = playerNames
            .Select(n => new GamePlayer
            {
                Name = n,
                Score = request.StartScore
            })
            .ToList()
    };

    _matches[match.Id] = match;
    return match;
}

    public bool TryGetMatch(string id, out GameMatch? match)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            match = null;
            return false;
        }
        return _matches.TryGetValue(id, out match);
    }

    public bool UpdateMatch(string id, GameMatch match)
    {
        if (string.IsNullOrWhiteSpace(id) || match == null)
            return false;

        if (_matches.TryGetValue(id, out var existingMatch))
        {
            lock (existingMatch)
            {
                _matches[id] = match;
            }
            return true;
        }   
        return false;
    }
}