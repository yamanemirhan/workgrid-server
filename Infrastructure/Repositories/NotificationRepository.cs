using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NotificationRepository(AppDbContext _appContext) : INotificationRepository
{
    public async Task<Notification> CreateAsync(Notification notification)
    {
        _appContext.Notifications.Add(notification);
        await _appContext.SaveChangesAsync();
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, int take = 50, int skip = 0)
    {
        return await _appContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
    {
        return await _appContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _appContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Notification> MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _appContext.Notifications.FindAsync(notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _appContext.SaveChangesAsync();
        }
        return notification!;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _appContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        var count = unreadNotifications.Count;
        var now = DateTime.UtcNow;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        if (count > 0)
        {
            await _appContext.SaveChangesAsync();
        }

        return count;
    }

    public async Task DeleteAsync(Guid id)
    {
        var notification = await _appContext.Notifications.FindAsync(id);
        if (notification != null)
        {
            _appContext.Notifications.Remove(notification);
            await _appContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Notification>> GetWorkspaceNotificationsAsync(Guid workspaceId, int take = 50)
    {
        return await _appContext.Notifications
            .Where(n => n.WorkspaceId == workspaceId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId)
    {
        return await _appContext.WorkspaceMembers
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId)
            .ToListAsync();
    }
}