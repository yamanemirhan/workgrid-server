using Domain.Entities;

namespace Infrastructure.Repositories;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, int take = 50, int skip = 0);
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification> MarkAsReadAsync(Guid notificationId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Notification>> GetWorkspaceNotificationsAsync(Guid workspaceId, int take = 50);
    Task<IEnumerable<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId);
}
