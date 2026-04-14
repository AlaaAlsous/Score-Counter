namespace Backend.Models;

public class GamePlayer
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
}