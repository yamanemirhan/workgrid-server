using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Repositories;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid id);
    Task<Workspace?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Workspace>> GetUserWorkspacesAsync(Guid userId);
    Task<Workspace> CreateAsync(Workspace workspace);
    Task<Workspace> UpdateAsync(Workspace workspace);
    Task DeleteAsync(Guid id);
    Task<bool> IsUserMemberOfWorkspaceAsync(Guid userId, Guid workspaceId);
    Task<bool> IsUserOwnerOfWorkspaceAsync(Guid userId, Guid workspaceId);
    Task<bool> IsUserAdminOfWorkspaceAsync(Guid userId, Guid workspaceId);
    Task<bool> IsUserWorkspaceMemberAsync(Guid userId, Guid workspaceId);
    Task<WorkspaceRole?> GetUserRoleInWorkspaceAsync(Guid userId, Guid workspaceId);
    Task<WorkspaceMember> AddMemberAsync(WorkspaceMember member);
    Task<int> GetMemberCountAsync(Guid workspaceId);
    Task<bool> IsWorkspaceMemberLimitExceededAsync(Guid workspaceId, int maxMembers = 50);
    Task<IEnumerable<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId);
    Task<WorkspaceMember> UpdateMemberRoleAsync(WorkspaceMember member);
    Task RemoveMemberAsync(Guid memberId);
    Task<IEnumerable<User>> SearchWorkspaceMembersAsync(Guid workspaceId, string searchTerm);
}