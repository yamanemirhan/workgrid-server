using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Member.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;

namespace Application.Member.Handlers;

internal class RemoveMemberHandler(IWorkspaceRepository _workspaceRepository,
     IHttpContextAccessor _httpContextAccessor,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<RemoveMemberCommand, bool>
{
    public async Task<bool> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
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

        var members = await _workspaceRepository.GetWorkspaceMembersAsync(request.WorkspaceId);
        var memberToRemove = members.FirstOrDefault(m => m.Id == request.MemberId);
        
        if (memberToRemove == null)
        {
            throw new ArgumentException("Member not found in this workspace");
        }

        var currentUserRole = await _workspaceRepository.GetUserRoleInWorkspaceAsync(currentUserId, request.WorkspaceId);

        if (memberToRemove.UserId == currentUserId)
        {
            await _workspaceRepository.RemoveMemberAsync(request.MemberId);
            
            var memberLeftEvent = new MemberLeftEvent
            {
                UserId = currentUserId,
                WorkspaceId = request.WorkspaceId,
                LeftUserId = currentUserId,
                LeftUserName = memberToRemove.User?.Name ?? "Unknown User",
                LeftUserEmail = memberToRemove.User?.Email ?? "unknown@email.com",
                PreviousRole = memberToRemove.Role,
                WorkspaceName = workspace.Name,
                Description = $"{memberToRemove.User?.Name ?? "User"} left workspace '{workspace.Name}'",
                Metadata = $"{{\"MemberId\":\"{request.MemberId}\",\"PreviousRole\":\"{memberToRemove.Role}\",\"LeftAt\":\"{DateTime.UtcNow:O}\"}}",
                ActivityType = ActivityType.MemberLeft
            };

            try
            {
                await _rabbitMqPublisher.PublishAsync(memberLeftEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish MemberLeftEvent: {ex.Message}");
            }

            var remainingMembers = await _workspaceRepository.GetMemberCountAsync(request.WorkspaceId);
            if (remainingMembers == 0)
            {
                await _workspaceRepository.DeleteAsync(request.WorkspaceId);
            }
            return true;
        }

        if (currentUserRole != WorkspaceRole.Owner && currentUserRole != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("You don't have permission to remove members. Only workspace owners and admins can remove members.");
        }

        if (memberToRemove.Role == WorkspaceRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove the workspace owner");
        }

        if (currentUserRole == WorkspaceRole.Admin && memberToRemove.Role == WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Admins cannot remove other admins. Only the workspace owner can remove admins.");
        }

        var currentUser = members.FirstOrDefault(m => m.UserId == currentUserId);

        await _workspaceRepository.RemoveMemberAsync(request.MemberId);

        var memberRemovedEvent = new MemberRemovedEvent
        {
            UserId = currentUserId,
            WorkspaceId = request.WorkspaceId,
            RemovedUserId = memberToRemove.UserId,
            RemovedUserName = memberToRemove.User?.Name ?? "Unknown User",
            RemovedUserEmail = memberToRemove.User?.Email ?? "unknown@email.com",
            RemovedBy = currentUserId,
            RemovedByName = currentUser?.User?.Name ?? "Unknown User",
            PreviousRole = memberToRemove.Role,
            WorkspaceName = workspace.Name,
            Description = $"{currentUser?.User?.Name ?? "User"} removed {memberToRemove.User?.Name ?? "user"} from workspace '{workspace.Name}'",
            Metadata = $"{{\"MemberId\":\"{request.MemberId}\",\"PreviousRole\":\"{memberToRemove.Role}\",\"RemovedAt\":\"{DateTime.UtcNow:O}\"}}",
            ActivityType = ActivityType.MemberRemoved
        };

        try
        {
            await _rabbitMqPublisher.PublishAsync(memberRemovedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish MemberRemovedEvent: {ex.Message}");
        }

        var remaining = await _workspaceRepository.GetMemberCountAsync(request.WorkspaceId);
        if (remaining == 0)
        {
            await _workspaceRepository.DeleteAsync(request.WorkspaceId);
        }
        return true;
    }
}