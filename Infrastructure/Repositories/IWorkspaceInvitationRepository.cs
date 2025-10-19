using Domain.Entities;

namespace Infrastructure.Repositories;

public interface IWorkspaceInvitationRepository
{
    Task<WorkspaceInvitation?> GetByIdAsync(Guid id);
    Task<WorkspaceInvitation?> GetByTokenAsync(string token);
    Task<WorkspaceInvitation?> GetByWorkspaceAndEmailAsync(Guid workspaceId, string email);
    Task<IEnumerable<WorkspaceInvitation>> GetWorkspaceInvitationsAsync(Guid workspaceId);
    Task<WorkspaceInvitation> CreateAsync(WorkspaceInvitation invitation);
    Task<WorkspaceInvitation> UpdateAsync(WorkspaceInvitation invitation);
    Task DeleteAsync(Guid id);
    Task<bool> IsEmailAlreadyInvitedAsync(Guid workspaceId, string email);
}