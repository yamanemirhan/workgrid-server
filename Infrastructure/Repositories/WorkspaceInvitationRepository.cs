using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class WorkspaceInvitationRepository(AppDbContext _context) : IWorkspaceInvitationRepository
{
    public async Task<WorkspaceInvitation?> GetByIdAsync(Guid id)
    {
        return await _context.WorkspaceInvitations
            .Include(wi => wi.Workspace)
            .Include(wi => wi.InvitedBy)
            .FirstOrDefaultAsync(wi => wi.Id == id);
    }

    public async Task<WorkspaceInvitation?> GetByTokenAsync(string token)
    {
        return await _context.WorkspaceInvitations
            .Include(wi => wi.Workspace)
            .Include(wi => wi.InvitedBy)
            .FirstOrDefaultAsync(wi => wi.Token == token && wi.Status == InvitationStatus.Pending && wi.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<WorkspaceInvitation?> GetByWorkspaceAndEmailAsync(Guid workspaceId, string email)
    {
        return await _context.WorkspaceInvitations
            .Include(wi => wi.Workspace)
            .Include(wi => wi.InvitedBy)
            .FirstOrDefaultAsync(wi => wi.WorkspaceId == workspaceId && 
                                      wi.Email.ToLower() == email.ToLower() && 
                                      wi.Status == InvitationStatus.Pending);
    }

    public async Task<IEnumerable<WorkspaceInvitation>> GetWorkspaceInvitationsAsync(Guid workspaceId)
    {
        return await _context.WorkspaceInvitations
            .Include(wi => wi.InvitedBy)
            .Where(wi => wi.WorkspaceId == workspaceId && wi.Status == InvitationStatus.Pending)
            .OrderByDescending(wi => wi.CreatedAt)
            .ToListAsync();
    }

    public async Task<WorkspaceInvitation> CreateAsync(WorkspaceInvitation invitation)
    {
        _context.WorkspaceInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task<WorkspaceInvitation> UpdateAsync(WorkspaceInvitation invitation)
    {
        _context.WorkspaceInvitations.Update(invitation);
        await _context.SaveChangesAsync();
        return invitation;
    }

    public async Task DeleteAsync(Guid id)
    {
        var invitation = await GetByIdAsync(id);
        if (invitation != null)
        {
            _context.WorkspaceInvitations.Remove(invitation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsEmailAlreadyInvitedAsync(Guid workspaceId, string email)
    {
        return await _context.WorkspaceInvitations
            .AnyAsync(wi => wi.WorkspaceId == workspaceId && 
                           wi.Email.ToLower() == email.ToLower() && 
                           wi.Status == InvitationStatus.Pending);
    }
}