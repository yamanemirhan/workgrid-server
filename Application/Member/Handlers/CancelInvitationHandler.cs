using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Member.Commands;
using Domain.Enums;

namespace Application.Member.Handlers;

internal class CancelInvitationHandler(IWorkspaceInvitationRepository _invitationRepository,
     IWorkspaceRepository _workspaceRepository,
     IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CancelInvitationCommand, bool>
{
    public async Task<bool> Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
    {
        // Get current user from JWT token
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Get invitation
        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
        if (invitation == null)
        {
            throw new ArgumentException("Invitation not found");
        }

        // Check if invitation is already processed
        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Invitation has already been processed and cannot be cancelled");
        }

        // Check if invitation has expired
        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Invitation has expired and cannot be cancelled");
        }

        // Check if current user has permission to cancel invitation (must be owner or admin)
        var isOwner = await _workspaceRepository.IsUserOwnerOfWorkspaceAsync(currentUserId, invitation.WorkspaceId);
        var isAdmin = await _workspaceRepository.IsUserAdminOfWorkspaceAsync(currentUserId, invitation.WorkspaceId);
        
        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You don't have permission to cancel this invitation. Only workspace owners and admins can cancel invitations.");
        }

        // Update invitation status to cancelled
        invitation.Status = InvitationStatus.Cancelled;
        invitation.UpdatedAt = DateTime.UtcNow;
        
        await _invitationRepository.UpdateAsync(invitation);

        return true;
    }
}