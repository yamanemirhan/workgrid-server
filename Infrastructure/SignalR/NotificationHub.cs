using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

[Authorize]
public class NotificationHub(INotificationRepository _notificationRepository) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdClaim = Context.User?.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkNotificationAsRead(Guid notificationId)
    {
    }

    public async Task JoinWorkspaceGroup(Guid workspaceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"workspace_{workspaceId}");
    }

    public async Task LeaveWorkspaceGroup(Guid workspaceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workspace_{workspaceId}");
    }
}