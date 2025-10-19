using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Shared.DTOs;
using Application.Member.Commands;
using Domain.Enums;

namespace Application.Member.Handlers;

internal class ResendInvitationHandler(IWorkspaceInvitationRepository _invitationRepository,
     IWorkspaceRepository _workspaceRepository,
     IUserRepository _userRepository,
     IHttpContextAccessor _httpContextAccessor,
     IEmailService _emailService) : IRequestHandler<ResendInvitationCommand, InvitationDto>
{
    public async Task<InvitationDto> Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
        if (invitation == null)
        {
            throw new ArgumentException("Invitation not found");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invitations can be resent");
        }

        if (invitation.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Expired invitations cannot be resent. Please create a new invitation.");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(invitation.WorkspaceId);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found");
        }

        var isOwner = await _workspaceRepository.IsUserOwnerOfWorkspaceAsync(currentUserId, invitation.WorkspaceId);
        var isAdmin = await _workspaceRepository.IsUserAdminOfWorkspaceAsync(currentUserId, invitation.WorkspaceId);
        
        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You don't have permission to resend this invitation. Only workspace owners and admins can resend invitations.");
        }

        invitation.Token = GenerateInvitationToken();
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
        invitation.UpdatedAt = DateTime.UtcNow;
        
        var updatedInvitation = await _invitationRepository.UpdateAsync(invitation);

        var inviter = await _userRepository.GetByIdAsync(currentUserId);
        var existingUser = await _userRepository.GetByEmailAsync(invitation.Email);

        try
        {
            await _emailService.SendInvitationEmailAsync(
                recipientEmail: updatedInvitation.Email,
                recipientName: existingUser?.Name,
                inviterName: inviter?.Name ?? "Unknown User",
                workspaceName: workspace.Name,
                invitationToken: updatedInvitation.Token,
                expiresAt: updatedInvitation.ExpiresAt
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invitation updated but email could not be sent: {ex.Message}");
        }

        return new InvitationDto
        {
            Id = updatedInvitation.Id,
            WorkspaceId = updatedInvitation.WorkspaceId,
            WorkspaceName = workspace.Name,
            Email = updatedInvitation.Email,
            Role = updatedInvitation.Role,
            InvitedByUserId = updatedInvitation.InvitedByUserId,
            InvitedByUserName = inviter?.Name ?? "Unknown",
            Token = updatedInvitation.Token,
            Status = updatedInvitation.Status,
            CreatedAt = updatedInvitation.CreatedAt,
            ExpiresAt = updatedInvitation.ExpiresAt
        };
    }

    private static string GenerateInvitationToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}