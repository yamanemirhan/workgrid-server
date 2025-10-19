using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardStatusRepository(AppDbContext _context) : ICardStatusRepository
{
    public async Task<IEnumerable<CardStatus>> GetWorkspaceStatusesAsync(Guid workspaceId)
    {
        // Get default statuses + workspace-specific custom statuses
        var defaultStatuses = await _context.CardStatuses
            .Where(s => s.IsDefault && s.WorkspaceId == null)
            .ToListAsync();

        var customStatuses = await _context.CardStatuses
            .Where(s => s.WorkspaceId == workspaceId && !s.IsDefault)
            .ToListAsync();

        return defaultStatuses.Concat(customStatuses).OrderBy(s => s.Position);
    }

    public async Task<IEnumerable<CardStatus>> GetDefaultStatusesAsync()
    {
        return await _context.CardStatuses
            .Where(s => s.IsDefault && s.WorkspaceId == null)
            .OrderBy(s => s.Position)
            .ToListAsync();
    }

    public async Task<CardStatus?> GetByIdAsync(Guid id)
    {
        return await _context.CardStatuses
            .Include(s => s.Workspace)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<CardStatus?> GetByNameAndWorkspaceAsync(string name, Guid? workspaceId)
    {
        return await _context.CardStatuses
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower() && s.WorkspaceId == workspaceId);
    }

    public async Task<CardStatus> CreateAsync(CardStatus cardStatus)
    {
        _context.CardStatuses.Add(cardStatus);
        await _context.SaveChangesAsync();
        return cardStatus;
    }

    public async Task<CardStatus> UpdateAsync(CardStatus cardStatus)
    {
        _context.CardStatuses.Update(cardStatus);
        await _context.SaveChangesAsync();
        return cardStatus;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var cardStatus = await _context.CardStatuses.FindAsync(id);
        if (cardStatus == null) return false;

        _context.CardStatuses.Remove(cardStatus);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.CardStatuses.AnyAsync(s => s.Id == id);
    }

    public async Task<bool> IsDefaultStatusAsync(Guid id)
    {
        var status = await _context.CardStatuses.FindAsync(id);
        return status?.IsDefault ?? false;
    }

    public async Task<CardStatus?> GetDefaultStatusByNameAsync(string name)
    {
        return await _context.CardStatuses
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower() && s.IsDefault && s.WorkspaceId == null);
    }

    public async Task<IEnumerable<CardStatus>> CreateDefaultStatusesForWorkspaceAsync(Guid workspaceId)
    {
        var defaultStatuses = new List<CardStatus>
        {
            new CardStatus
            {
                Name = "To-Do",
                Description = "Tasks that need to be started",
                Color = "#6B7280", // Gray
                Position = 1,
                IsDefault = true,
                Type = CardStatusType.Default,
                WorkspaceId = null, // Default statuses are global
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CardStatus
            {
                Name = "In Progress",
                Description = "Tasks currently being worked on",
                Color = "#3B82F6", // Blue
                Position = 2,
                IsDefault = true,
                Type = CardStatusType.Default,
                WorkspaceId = null, // Default statuses are global
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new CardStatus
            {
                Name = "Done",
                Description = "Completed tasks",
                Color = "#10B981", // Green
                Position = 3,
                IsDefault = true,
                Type = CardStatusType.Default,
                WorkspaceId = null, // Default statuses are global
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Check if default statuses already exist in database
        var existingDefaults = await GetDefaultStatusesAsync();
        var statusesToCreate = new List<CardStatus>();

        foreach (var defaultStatus in defaultStatuses)
        {
            var exists = existingDefaults.Any(s => s.Name == defaultStatus.Name);
            if (!exists)
            {
                statusesToCreate.Add(defaultStatus);
            }
        }

        if (statusesToCreate.Any())
        {
            _context.CardStatuses.AddRange(statusesToCreate);
            await _context.SaveChangesAsync();
        }

        // Return all default statuses (existing + newly created)
        return await GetDefaultStatusesAsync();
    }

    public async Task<int> GetCardCountByStatusAsync(Guid statusId)
    {
        return await _context.Cards
            .Where(c => c.StatusId == statusId && !c.IsDeleted)
            .CountAsync();
    }

    public async Task<bool> CanDeleteStatusAsync(Guid statusId)
    {
        // Can't delete if any cards are using this status
        var cardCount = await GetCardCountByStatusAsync(statusId);
        return cardCount == 0;
    }
}