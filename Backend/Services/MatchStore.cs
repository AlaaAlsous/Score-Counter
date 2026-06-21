using Microsoft.EntityFrameworkCore;
using Shared;
using Backend.Database;

namespace Backend.Services;

public class MatchStore
{
    private readonly AppDbContext _db;

    public MatchStore(AppDbContext db)
    {
        _db = db;
    }

    private string GenerateUniqueId()
    {
        var rnd = Random.Shared;

        for (int i = 0; i < 20; i++)
        {
            var id = IdGenerator.Generate(rnd);

            if (!_db.Matches.Any(m => m.Id == id))
                return id;
        }
        return Guid.NewGuid().ToString("N")[..12];
    }

    public GameMatch CreateMatch(MatchRequestDto request)
    {
        var playerNames = request.PlayerNames?
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .ToList() ?? new List<string>();

        var match = new GameMatch
        {
            Id = GenerateUniqueId(),
            GameName = request.GameName,
            HighScoreWins = request.HighScoreWins,
            MaxPlayers = request.MaxPlayers,
            PlayersLocked = request.PlayersLocked,
            StartScore = request.StartScore,
            MaxScore = request.MaxScore,

            OriginalPlayerNames = playerNames.ToList(),
            Players = playerNames
                .Select(n => new GamePlayer
                {
                    Name = n,
                    Score = request.StartScore
                })
                .ToList()
        };

        _db.Matches.Add(match);
        _db.SaveChanges();
        return match;
    }

    public bool TryGetMatch(string id, out GameMatch? match)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            match = null;
            return false;
        }

        match = _db.Matches
            .Include(m => m.Players)
            .FirstOrDefault(m => m.Id == id);

        return match != null;
    }

    public bool UpdateMatch(string id, GameMatch updated)
    {
        if (string.IsNullOrWhiteSpace(id) || updated == null)
            return false;

        var existing = _db.Matches
            .Include(m => m.Players)
            .FirstOrDefault(m => m.Id == id);

        if (existing == null)
            return false;

        existing.IsFinished = updated.IsFinished;
        existing.PlayersLocked = updated.PlayersLocked;
        existing.HighScoreWins = updated.HighScoreWins;

        foreach (var updatedPlayer in updated.Players)
        {
            var existingPlayer = existing.Players.FirstOrDefault(p => p.Id == updatedPlayer.Id);
            if (existingPlayer != null)
                existingPlayer.Score = updatedPlayer.Score;
        }

        _db.SaveChanges();
        return true;
    }

    public bool AddPlayer(string matchId, string playerName, out GamePlayer? newPlayer)
    {
        newPlayer = null;
        var match = _db.Matches.Include(m => m.Players).FirstOrDefault(m => m.Id == matchId);
        if (match == null) return false;
        if (match.IsFinished || match.PlayersLocked) return false;
        if (match.MaxPlayers > 0 && match.Players.Count >= match.MaxPlayers) return false;
        if (match.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))) return false;

        newPlayer = new GamePlayer { Name = playerName.Trim(), Score = match.StartScore, GameMatchId = matchId };
        _db.Players.Add(newPlayer);
        _db.SaveChanges();
        return true;
    }

    public bool RemovePlayer(string matchId, Guid playerId, out GamePlayer? removed, out string playerName)
    {
        playerName = string.Empty;
        removed = _db.Players.FirstOrDefault(p => p.Id == playerId && p.GameMatchId == matchId);
        if (removed == null) return false;

        playerName = removed.Name;

        var historyEntries = _db.ScoreEntries.Where(e => e.GameMatchId == matchId && e.PlayerId == playerId);
        _db.ScoreEntries.RemoveRange(historyEntries);

        _db.Players.Remove(removed);
        _db.SaveChanges();
        return true;
    }

    public bool UpdatePlayerName(string matchId, Guid playerId, string newName, out GamePlayer? updated)
    {
        updated = null;
        var match = _db.Matches.Include(m => m.Players).FirstOrDefault(m => m.Id == matchId);
        if (match == null) return false;
        if (match.IsFinished) return false;

        updated = match.Players.FirstOrDefault(p => p.Id == playerId);
        if (updated == null) return false;

        if (match.Players.Any(p => p.Id != playerId && p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            return false;

        updated.Name = newName.Trim();

        var entries = _db.ScoreEntries.Where(e => e.GameMatchId == matchId && e.PlayerId == playerId);
        foreach (var entry in entries)
            entry.PlayerName = newName.Trim();

        _db.SaveChanges();
        return true;
    }

    public bool FinishMatch(string matchId, out GameMatch? match)
    {
        match = _db.Matches.Include(m => m.Players).FirstOrDefault(m => m.Id == matchId);
        if (match == null) return false;

        match.IsFinished = true;
        _db.SaveChanges();
        return true;
    }

    public bool UpdatePlayerScore(string matchId, Guid playerId, int? newScore, int? amount, string? operation, out GamePlayer? updated, out ScoreEntry? entry)
    {
        entry = null;
        updated = _db.Players.FirstOrDefault(p => p.Id == playerId && p.GameMatchId == matchId);
        if (updated == null) return false;

        var scoreBefore = updated.Score;

        if (newScore.HasValue)
            updated.Score = newScore.Value;
        else if (amount.HasValue && !string.IsNullOrWhiteSpace(operation))
            updated.Score += operation == "decrease" ? -amount.Value : amount.Value;

        var delta = updated.Score - scoreBefore;
        if (delta != 0)
        {
            entry = new ScoreEntry
            {
                GameMatchId = matchId,
                PlayerId = playerId,
                PlayerName = updated.Name,
                ScoreBefore = scoreBefore,
                ScoreAfter = updated.Score,
                Delta = delta,
                Timestamp = DateTime.UtcNow
            };
            _db.ScoreEntries.Add(entry);
        }

        _db.SaveChanges();
        return true;
    }

    public List<ScoreEntry> GetScoreHistory(string matchId)
    {
        return _db.ScoreEntries
            .Where(e => e.GameMatchId == matchId)
            .OrderByDescending(e => e.Timestamp)
            .ToList();
    }

    public bool CheckNameForUniqueness(string matchId, string newName, string playerId)
    {
        if (!Guid.TryParse(playerId, out var playerGuid))
            return false;

        var match = _db.Matches
            .Include(m => m.Players)
            .FirstOrDefault(m => m.Id == matchId);

        if (match == null)
            return false;

        var trimmedName = newName.Trim();

        var exists = match.Players.Any(p =>
            p.Id != playerGuid &&
            p.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        return !exists;
    }
}
