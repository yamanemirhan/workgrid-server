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

internal class AcceptInvitationHandler(IWorkspaceInvitationRepository _invitationRepository,
     IWorkspaceRepository _workspaceRepository,
     IUserRepository _userRepository,
     IHttpContextAccessor _httpContextAccessor,
     IEmailService _emailService,
     IRateLimitingService _rateLimitingService,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<AcceptInvitationCommand, MemberDto>
{
    public async Task<MemberDto> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        string? userIdClaim = null;
        
        try
        {
            // 1. Rate limiting check
            var rateLimitKey = $"accept_invitation_{clientIp}";
            var isAllowed = await _rateLimitingService.IsAllowedAsync(rateLimitKey, 10, TimeSpan.FromMinutes(15));
            if (!isAllowed)
            {
                throw new InvalidOperationException("Too many invitation acceptance attempts. Please try again later.");
            }

            // Record this attempt
            await _rateLimitingService.RecordAttemptAsync(rateLimitKey, TimeSpan.FromMinutes(15));

            // 2. Validate token format
            if (!IsValidTokenFormat(request.Token))
            {
                throw new ArgumentException("Invalid invitation token format");
            }

            // 3. Get current user from JWT token
            userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Additional rate limiting per user
            var userRateLimitKey = $"accept_invitation_user_{userId}";
            var userAllowed = await _rateLimitingService.IsAllowedAsync(userRateLimitKey, 5, TimeSpan.FromMinutes(15));
            if (!userAllowed)
            {
                throw new InvalidOperationException("Too many invitation acceptance attempts. Please try again later.");
            }
            await _rateLimitingService.RecordAttemptAsync(userRateLimitKey, TimeSpan.FromMinutes(15));

            // 4. Get the invitation by token with comprehensive validation
            var invitation = await _invitationRepository.GetByTokenAsync(request.Token);
            if (invitation == null)
            {
                throw new ArgumentException("Invalid or expired invitation token");
            }

            // 5. Validate invitation status
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

            // 6. Check expiration
            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {                
                // Mark as expired
                invitation.Status = InvitationStatus.Expired;
                await _invitationRepository.UpdateAsync(invitation);
                
                throw new InvalidOperationException("This invitation has expired. Please request a new invitation.");
            }

            // 7. Get user details and validate
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // 8. Validate email match (case-insensitive)
            if (!string.Equals(user.Email.Trim(), invitation.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("This invitation is for a different email address");
            }

            // 9. Check if user is already a member
            var isAlreadyMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, invitation.WorkspaceId);
            if (isAlreadyMember)
            {
                // Mark invitation as accepted since user is already a member
                invitation.Status = InvitationStatus.Accepted;
                await _invitationRepository.UpdateAsync(invitation);
                
                throw new InvalidOperationException("You are already a member of this workspace");
            }

            // 10. Get workspace and validate
            var workspace = await _workspaceRepository.GetByIdAsync(invitation.WorkspaceId);
            if (workspace == null)
            {
                throw new ArgumentException("Workspace no longer exists");
            }

            // 11. Check workspace member limits
            var isLimitExceeded = await _workspaceRepository.IsWorkspaceMemberLimitExceededAsync(invitation.WorkspaceId);
            if (isLimitExceeded)
            {
                throw new InvalidOperationException("This workspace has reached its maximum member limit");
            }

            // 12. Create WorkspaceMember record with transaction safety
            var member = new WorkspaceMember
            {
                UserId = userId,
                WorkspaceId = invitation.WorkspaceId,
                Role = invitation.Role,
                JoinedAt = DateTime.UtcNow
            };

            try
            {
                // Add member and update invitation in a transaction-like manner
                var createdMember = await _workspaceRepository.AddMemberAsync(member);
                
                // Update invitation status to accepted
                invitation.Status = InvitationStatus.Accepted;
                invitation.UpdatedAt = DateTime.UtcNow;
                await _invitationRepository.UpdateAsync(invitation);

                // 13. Send welcome email (don't fail if email fails)
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
                }
                catch (Exception emailEx)
                {
                    // Don't throw
                }

                // 14. Publish MemberJoinedEvent for activity logging
                var memberJoinedEvent = new MemberJoinedEvent
                {
                    UserId = userId, // The user who joined
                    WorkspaceId = invitation.WorkspaceId,
                    JoinedUserId = userId,
                    JoinedUserName = user.Name,
                    JoinedUserEmail = user.Email,
                    Role = invitation.Role,
                    WorkspaceName = workspace.Name,
                    Description = $"{user.Name} joined workspace '{workspace.Name}' as {invitation.Role}",
                    Metadata = $"{{\"InvitationId\":\"{invitation.Id}\",\"AcceptedAt\":\"{DateTime.UtcNow:O}\",\"Role\":\"{invitation.Role}\"}}",
                    ActivityType = ActivityType.MemberJoined
                };

                try
                {
                    await _rabbitMqPublisher.PublishAsync(memberJoinedEvent);
                }
                catch (Exception ex)
                {
                    // Don't throw, just log the warning - activity logging failure shouldn't prevent membership
                }

                // 15. Log successful acceptance

                // 16. Return successful response
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

    /// <summary>
    /// Validates if the token has the expected format (64 characters, alphanumeric)
    /// </summary>
    private static bool IsValidTokenFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        // Token should be 64 characters (32+32 from two GUIDs without dashes)
        if (token.Length != 64)
            return false;

        // Should only contain alphanumeric characters
        return Regex.IsMatch(token, "^[a-zA-Z0-9]+$");
    }
}