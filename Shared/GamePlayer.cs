namespace Shared;

public class GamePlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
    public string GameMatchId { get; set; } = string.Empty;
}
