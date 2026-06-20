namespace Shared;

public class ScoreEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string GameMatchId { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int ScoreBefore { get; set; }
    public int ScoreAfter { get; set; }
    public int Delta { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}