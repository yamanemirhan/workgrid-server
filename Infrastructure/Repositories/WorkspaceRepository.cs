using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class WorkspaceRepository(AppDbContext _context) : IWorkspaceRepository
{
    public async Task<Workspace?> GetByIdAsync(Guid id)
    {
        return await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<Workspace?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Workspaces
            .Include(w => w.Owner)
            .Include(w => w.Members)
                .ThenInclude(m => m.User)
            .Include(w => w.Boards
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.CreatedAt))
            .Include(w => w.Subscription)
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }
    public async Task<IEnumerable<Workspace>> GetUserWorkspacesAsync(Guid userId)
    {
        return await _context.Workspaces
            .Include(w => w.Owner)
            .Include(w => w.Members)
                .ThenInclude(m => m.User)
            .Include(w => w.Boards
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.CreatedAt))
            .Include(w => w.Subscription)
            .Where(w =>
                (w.OwnerId == userId || w.Members.Any(m => m.UserId == userId))
                && !w.IsDeleted)
            .ToListAsync();
    }

    public async Task<Workspace> CreateAsync(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();
        return workspace;
    }

    public async Task<Workspace> UpdateAsync(Workspace workspace)
    {
        _context.Workspaces.Update(workspace);
        await _context.SaveChangesAsync();
        return workspace;
    }

    public async Task DeleteAsync(Guid id)
    {
        var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == id);
        if (workspace != null)
        {
            workspace.IsDeleted = true;
            _context.Workspaces.Update(workspace);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserMemberOfWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        return await _context.WorkspaceMembers
                    .AnyAsync(wm => wm.UserId == userId
                && wm.WorkspaceId == workspaceId);
    }

    public async Task<bool> IsUserOwnerOfWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(wm => wm.UserId == userId && 
                           wm.WorkspaceId == workspaceId && 
                           wm.Role == WorkspaceRole.Owner);
    }

    public async Task<bool> IsUserAdminOfWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(wm => wm.UserId == userId && 
                           wm.WorkspaceId == workspaceId && 
                           (wm.Role == WorkspaceRole.Owner || wm.Role == WorkspaceRole.Admin));
    }

    public async Task<bool> IsUserWorkspaceMemberAsync(Guid userId, Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(wm => wm.UserId == userId && 
                           wm.WorkspaceId == workspaceId &&
                           !wm.Workspace.IsDeleted);
    }

    public async Task<WorkspaceRole?> GetUserRoleInWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(wm =>
                wm.UserId == userId &&
                wm.WorkspaceId == workspaceId &&
                !wm.Workspace.IsDeleted);

        return member?.Role;
    }

    public async Task<WorkspaceMember> AddMemberAsync(WorkspaceMember member)
    {
        _context.WorkspaceMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<int> GetMemberCountAsync(Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .CountAsync(wm => wm.WorkspaceId == workspaceId
                              && !wm.Workspace.IsDeleted);
    }

    public async Task<bool> IsWorkspaceMemberLimitExceededAsync(Guid workspaceId, int maxMembers = 50)
    {
        var memberCount = await GetMemberCountAsync(workspaceId);
        return memberCount >= maxMembers;
    }

    public async Task<IEnumerable<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId
                         && !wm.Workspace.IsDeleted)
            .OrderBy(wm => wm.Role)
            .ThenBy(wm => wm.JoinedAt)
            .ToListAsync();
    }

    public async Task<WorkspaceMember> UpdateMemberRoleAsync(WorkspaceMember member)
    {
        _context.WorkspaceMembers.Update(member);
        await _context.SaveChangesAsync();
        
        return await _context.WorkspaceMembers
            .Include(wm => wm.User)
            .FirstOrDefaultAsync(wm => wm.Id == member.Id) ?? member;
    }

    public async Task RemoveMemberAsync(Guid memberId)
    {
        var member = await _context.WorkspaceMembers.FindAsync(memberId);
        if (member != null)
        {
            _context.WorkspaceMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<User>> SearchWorkspaceMembersAsync(Guid workspaceId, string searchTerm)
    {
        var query = _context.WorkspaceMembers
            .Include(wm => wm.User)
            .Where(wm => wm.WorkspaceId == workspaceId && !wm.Workspace.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(wm => 
                wm.User.Name.ToLower().Contains(lowerSearchTerm) ||
                wm.User.Email.ToLower().Contains(lowerSearchTerm));
        }

        var members = await query
            .OrderBy(wm => wm.User.Name)
            .Take(10) // Limit results for performance
            .ToListAsync();

        return members.Select(wm => wm.User);
    }
}
