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
using System.Text.RegularExpressions;

namespace Application.Member.Handlers;

internal class AcceptPublicInvitationHandler(IWorkspaceInvitationRepository _invitationRepository,
     IWorkspaceRepository _workspaceRepository,
     IUserRepository _userRepository,
     IHttpContextAccessor _httpContextAccessor,
     IEmailService _emailService,
     IRateLimitingService _rateLimitingService,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<AcceptPublicInvitationCommand, MemberDto>
{
    public async Task<MemberDto> Handle(AcceptPublicInvitationCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        
        try
        {
            // 1. Rate limiting check
            var rateLimitKey = $"accept_public_invitation_{clientIp}";
            var isAllowed = await _rateLimitingService.IsAllowedAsync(rateLimitKey, 5, TimeSpan.FromMinutes(15));
            if (!isAllowed)
            {
                throw new InvalidOperationException("Too many invitation acceptance attempts. Please try again later.");
            }

            await _rateLimitingService.RecordAttemptAsync(rateLimitKey, TimeSpan.FromMinutes(15));

            // 2. Validate inputs
            if (!IsValidTokenFormat(request.Token))
            {
                throw new ArgumentException("Invalid invitation token format");
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            {
                throw new ArgumentException("Valid email address is required");
            }

            // 3. Get the invitation by token
            var invitation = await _invitationRepository.GetByTokenAsync(request.Token);
            if (invitation == null)
            {
                throw new ArgumentException("Invalid or expired invitation token");
            }

            // 4. Validate invitation status and expiration
            if (invitation.Status != InvitationStatus.Pending)
            {               
                var statusMessage = invitation.Status switch
                {
                    InvitationStatus.Accepted => "This invitation has already been accepted",
                    InvitationStatus.Cancelled => "This invitation has been cancelled",
                    InvitationStatus.Rejected => "This invitation has been rejected",
                    InvitationStatus.Expired => "This invitation has expired",
                    _ => "This invitation is no longer valid"
                };
                throw new InvalidOperationException(statusMessage);
            }

            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {                
                invitation.Status = InvitationStatus.Expired;
                await _invitationRepository.UpdateAsync(invitation);
                
                throw new InvalidOperationException("This invitation has expired. Please request a new invitation.");
            }

            // 5. Validate email match with invitation
            if (!string.Equals(request.Email.Trim(), invitation.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("This invitation is for a different email address");
            }

            // 6. Get or suggest user registration
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException($"No account found with email {request.Email}. Please register first, then accept the invitation.");
            }

            // 7. Check if user is already a member
            var isAlreadyMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(user.Id, invitation.WorkspaceId);
            if (isAlreadyMember)
            {
                invitation.Status = InvitationStatus.Accepted;
                await _invitationRepository.UpdateAsync(invitation);
                
                throw new InvalidOperationException("You are already a member of this workspace");
            }

            // 8. Get workspace and validate
            var workspace = await _workspaceRepository.GetByIdAsync(invitation.WorkspaceId);
            if (workspace == null)
            {
                throw new ArgumentException("Workspace no longer exists");
            }

            // 9. Check workspace member limits
            var isLimitExceeded = await _workspaceRepository.IsWorkspaceMemberLimitExceededAsync(invitation.WorkspaceId);
            if (isLimitExceeded)
            {
                throw new InvalidOperationException("This workspace has reached its maximum member limit");
            }

            // 10. Create WorkspaceMember record
            var member = new WorkspaceMember
            {
                UserId = user.Id,
                WorkspaceId = invitation.WorkspaceId,
                Role = invitation.Role,
                JoinedAt = DateTime.UtcNow
            };

            try
            {
                var createdMember = await _workspaceRepository.AddMemberAsync(member);
                
                invitation.Status = InvitationStatus.Accepted;
                invitation.UpdatedAt = DateTime.UtcNow;
                await _invitationRepository.UpdateAsync(invitation);

                // Send welcome email
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
                }
                catch (Exception emailEx)
                {
                }

                // Publish MemberJoinedEvent for activity logging
                var memberJoinedEvent = new MemberJoinedEvent
                {
                    UserId = user.Id, // The user who joined
                    WorkspaceId = invitation.WorkspaceId,
                    JoinedUserId = user.Id,
                    JoinedUserName = user.Name,
                    JoinedUserEmail = user.Email,
                    Role = invitation.Role,
                    WorkspaceName = workspace.Name,
                    Description = $"{user.Name} joined workspace '{workspace.Name}' as {invitation.Role} via public invitation",
                    Metadata = $"{{\"InvitationId\":\"{invitation.Id}\",\"AcceptedAt\":\"{DateTime.UtcNow:O}\",\"Role\":\"{invitation.Role}\",\"AcceptanceType\":\"Public\"}}"
                };

                try
                {
                    await _rabbitMqPublisher.PublishAsync(memberJoinedEvent);
                }
                catch (Exception ex)
                {
                    // Don't throw - activity logging failure shouldn't prevent membership
                }

                return new MemberDto
                {
                    Id = createdMember.Id,
                    UserId = createdMember.UserId,
                    WorkspaceId = createdMember.WorkspaceId,
                    UserName = user.Name,
                    UserEmail = user.Email,
                    UserAvatar = user.Avatar,
                    Role = createdMember.Role,
                    JoinedAt = createdMember.JoinedAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to accept invitation. Please try again.");
            }
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            throw;
        }
    }

    private static bool IsValidTokenFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        if (token.Length != 64)
            return false;

        return Regex.IsMatch(token, "^[a-zA-Z0-9]+$");
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