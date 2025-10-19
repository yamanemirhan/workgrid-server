using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Shared.DTOs;
using Application.Member.Commands;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;

namespace Application.Member.Handlers;

internal class InviteMemberHandler(IWorkspaceInvitationRepository _invitationRepository,
     IWorkspaceRepository _workspaceRepository,
     IUserRepository _userRepository,
     IHttpContextAccessor _httpContextAccessor,
     IEmailService _emailService,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<InviteMemberCommand, InvitationDto>
{
    public async Task<InvitationDto> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
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

        var isOwner = await _workspaceRepository.IsUserOwnerOfWorkspaceAsync(currentUserId, request.WorkspaceId);
        var isAdmin = await _workspaceRepository.IsUserAdminOfWorkspaceAsync(currentUserId, request.WorkspaceId);
        
        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You don't have permission to invite members to this workspace. Only workspace owners and admins can send invitations.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
        {
            throw new ArgumentException("Invalid email address");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(existingUser.Id, request.WorkspaceId);
            if (isMember)
            {
                throw new ArgumentException("User is already a member of this workspace");
            }
        }

        var existingInvitation = await _invitationRepository.IsEmailAlreadyInvitedAsync(request.WorkspaceId, request.Email);
        if (existingInvitation)
        {
            throw new ArgumentException("This email has already been invited to the workspace");
        }

        var invitation = new WorkspaceInvitation
        {
            WorkspaceId = request.WorkspaceId,
            Email = request.Email.ToLower(),
            Role = request.Role,
            InvitedByUserId = currentUserId,
            Token = GenerateInvitationToken(),
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var createdInvitation = await _invitationRepository.CreateAsync(invitation);

        var inviter = await _userRepository.GetByIdAsync(currentUserId);

        try
        {
            await _emailService.SendInvitationEmailAsync(
                recipientEmail: createdInvitation.Email,
                recipientName: existingUser?.Name,
                inviterName: inviter?.Name ?? "Unknown User",
                workspaceName: workspace.Name,
                invitationToken: createdInvitation.Token,
                expiresAt: createdInvitation.ExpiresAt
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invitation created but email could not be sent: {ex.Message}");
        }

        var memberInvitedEvent = new MemberInvitedEvent
        {
            UserId = currentUserId,
            WorkspaceId = request.WorkspaceId,
            InvitedUserId = existingUser?.Id ?? Guid.Empty,
            InvitedBy = currentUserId,
            InvitedEmail = createdInvitation.Email,
            InvitedUserName = existingUser?.Name,
            InviterName = inviter?.Name ?? "Unknown User",
            WorkspaceName = workspace.Name,
            Role = request.Role,
            Token = createdInvitation.Token,
            Description = $"{inviter?.Name ?? "User"} invited {createdInvitation.Email} to workspace '{workspace.Name}' as {request.Role}",
            Metadata = $"{{\"InvitationId\":\"{createdInvitation.Id}\",\"ExpiresAt\":\"{createdInvitation.ExpiresAt:O}\",\"Role\":\"{request.Role}\"}}"
        };

        try
        {
            await _rabbitMqPublisher.PublishAsync(memberInvitedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish MemberInvitedEvent: {ex.Message}");
        }
        
        return new InvitationDto
        {
            Id = createdInvitation.Id,
            WorkspaceId = createdInvitation.WorkspaceId,
            WorkspaceName = workspace.Name,
            Email = createdInvitation.Email,
            Role = createdInvitation.Role,
            InvitedByUserId = createdInvitation.InvitedByUserId,
            InvitedByUserName = inviter?.Name ?? "Unknown",
            Token = createdInvitation.Token,
            Status = createdInvitation.Status,
            CreatedAt = createdInvitation.CreatedAt,
            ExpiresAt = createdInvitation.ExpiresAt
        };
    }

    private static string GenerateInvitationToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}