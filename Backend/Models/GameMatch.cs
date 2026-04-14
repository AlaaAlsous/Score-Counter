namespace Backend.Models;

public class GameMatch
{
    public string id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string Name { get; set; } = string.Empty;
    public bool HighScoreWins { get; set; } = true;
    public bool PlayersLocked { get; set; } = false;
    public List<GamePlayer> Players { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}