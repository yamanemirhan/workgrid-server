using Domain.Entities;
using ActivityService.Data;
using Microsoft.EntityFrameworkCore;

namespace ActivityService.Repositories;

public class ActivityRepository(ActivityDbContext _context) : IActivityRepository
{
    public async Task<Activity> AddAsync(Activity activity)
    {
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByWorkspaceIdAsync(Guid workspaceId, int page = 1, int pageSize = 50)
    {
        return await _context.Activities
            .Where(a => a.WorkspaceId == workspaceId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByBoardIdAsync(Guid boardId, int page = 1, int pageSize = 50)
    {
        return await _context.Activities
            .Where(a => a.BoardId == boardId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByListIdAsync(Guid listId, int page = 1, int pageSize = 50)
    {
        return await _context.Activities
            .Where(a => a.ListId == listId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByCardIdAsync(Guid cardId, int page = 1, int pageSize = 50)
    {
        return await _context.Activities
            .Where(a => a.CardId == cardId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }     
}