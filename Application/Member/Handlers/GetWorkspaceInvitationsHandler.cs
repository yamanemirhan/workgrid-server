using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Member.Queries;

namespace Application.Member.Handlers;

internal class GetWorkspaceInvitationsHandler(IWorkspaceInvitationRepository _invitationRepository,
     IWorkspaceRepository _workspaceRepository,
     IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetWorkspaceInvitationsQuery, IEnumerable<InvitationDto>>
{
    public async Task<IEnumerable<InvitationDto>> Handle(GetWorkspaceInvitationsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found");
        }

        // Check if current user has permission to view invitations (must be owner or admin)
        var isOwner = await _workspaceRepository.IsUserOwnerOfWorkspaceAsync(currentUserId, request.WorkspaceId);
        var isAdmin = await _workspaceRepository.IsUserAdminOfWorkspaceAsync(currentUserId, request.WorkspaceId);
        
        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You don't have permission to view invitations for this workspace. Only workspace owners and admins can view invitations.");
        }

        var invitations = await _invitationRepository.GetWorkspaceInvitationsAsync(request.WorkspaceId);

        return invitations.Select(invitation => new InvitationDto
        {
            Id = invitation.Id,
            WorkspaceId = invitation.WorkspaceId,
            WorkspaceName = workspace.Name,
            Email = invitation.Email,
            Role = invitation.Role,
            InvitedByUserId = invitation.InvitedByUserId,
            InvitedByUserName = invitation.InvitedBy?.Name ?? "Unknown",
            Token = invitation.Token,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt
        });
    }
}