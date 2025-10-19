using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Member.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;

namespace Application.Member.Handlers;

internal class UpdateMemberRoleHandler(IWorkspaceRepository _workspaceRepository,
     IHttpContextAccessor _httpContextAccessor,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UpdateMemberRoleCommand, MemberDto>
{
    public async Task<MemberDto> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
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

        var currentUserRole = await _workspaceRepository.GetUserRoleInWorkspaceAsync(currentUserId, request.WorkspaceId);
        if (currentUserRole == null)
        {
            throw new UnauthorizedAccessException("You are not a member of this workspace.");
        }

        var members = await _workspaceRepository.GetWorkspaceMembersAsync(request.WorkspaceId);
        var memberToUpdate = members.FirstOrDefault(m => m.Id == request.MemberId);
        
        if (memberToUpdate == null)
        {
            throw new ArgumentException("Member not found in this workspace");
        }

        var oldRole = memberToUpdate.Role;

        if (memberToUpdate.Role == WorkspaceRole.Owner)
        {
            throw new InvalidOperationException("Cannot change the role of the workspace owner");
        }

        // Member cannot change anyone's role
        if (currentUserRole == WorkspaceRole.Member)
        {
            throw new UnauthorizedAccessException("Members cannot change anyone's role.");
        }

        // Admin cannot promote anyone to Owner
        if (currentUserRole == WorkspaceRole.Admin && request.NewRole == WorkspaceRole.Owner)
        {
            throw new UnauthorizedAccessException("Admins cannot promote anyone to Owner. Only the current Owner can do this.");
        }

        // Only Owner can promote someone to Owner
        if (request.NewRole == WorkspaceRole.Owner && currentUserRole != WorkspaceRole.Owner)
        {
            throw new UnauthorizedAccessException("Only the workspace owner can transfer ownership.");
        }

        memberToUpdate.Role = request.NewRole;
        memberToUpdate.UpdatedAt = DateTime.UtcNow;

        // Save the updated member
        var updatedMember = await _workspaceRepository.UpdateMemberRoleAsync(memberToUpdate);

        // Get current user info for event
        var currentUser = members.FirstOrDefault(m => m.UserId == currentUserId);

        var memberRoleChangedEvent = new MemberRoleChangedEvent
        {
            UserId = currentUserId,
            WorkspaceId = request.WorkspaceId,
            TargetUserId = memberToUpdate.UserId,
            TargetUserName = updatedMember.User?.Name ?? "Unknown User",
            TargetUserEmail = updatedMember.User?.Email ?? "unknown@email.com",
            ChangedBy = currentUserId,
            ChangedByName = currentUser?.User?.Name ?? "Unknown User",
            OldRole = oldRole,
            NewRole = request.NewRole,
            WorkspaceName = workspace.Name,
            Description = $"{currentUser?.User?.Name ?? "User"} changed {updatedMember.User?.Name ?? "user"}'s role from {oldRole} to {request.NewRole} in workspace '{workspace.Name}'",
            Metadata = $"{{\"MemberId\":\"{request.MemberId}\",\"OldRole\":\"{oldRole}\",\"NewRole\":\"{request.NewRole}\",\"ChangedAt\":\"{DateTime.UtcNow:O}\"}}"
        };

        try
        {
            await _rabbitMqPublisher.PublishAsync(memberRoleChangedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish MemberRoleChangedEvent: {ex.Message}");
        }

        return new MemberDto
        {
            Id = updatedMember.Id,
            UserId = updatedMember.UserId,
            WorkspaceId = updatedMember.WorkspaceId,
            UserName = updatedMember.User?.Name ?? "Unknown User",
            UserEmail = updatedMember.User?.Email ?? "unknown@email.com",
            UserAvatar = updatedMember.User?.Avatar,
            Role = updatedMember.Role,
            JoinedAt = updatedMember.JoinedAt
        };
    }
}