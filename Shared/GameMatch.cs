namespace Shared;

public class GameMatch
{
    public string Id { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public bool HighScoreWins { get; set; } = true;
    public bool PlayersLocked { get; set; } = false;
    public int MaxPlayers { get; set; } = 0;
    public int StartScore { get; set; } = 0;
    public List<GamePlayer> Players { get; set; } = new();
    public List<string> OriginalPlayerNames { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsFinished { get; set; } = false;
}