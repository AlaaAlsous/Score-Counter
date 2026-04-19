using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

class MatchEventHub : Hub
{
    public async Task JoinMatch(string matchId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);

    public async Task LeaveMatch(string matchId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
}
