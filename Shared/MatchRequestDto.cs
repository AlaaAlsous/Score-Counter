namespace Shared;

public class MatchRequestDto
{
    public string GameName { get; set; } = string.Empty;
    public bool HighScoreWins { get; set; } = true;
    public int MaxPlayers { get; set; } = 4;
    public bool PlayersLocked { get; set; } = false;
    public int StartScore { get; set; } = 0;
    public List<string>? PlayerNames { get; set; }
}
