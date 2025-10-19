using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Member.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;

namespace Application.Member.Handlers;

internal class LeaveWorkspaceHandler(IWorkspaceRepository _workspaceRepository,
    IHttpContextAccessor _httpContextAccessor,
    IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<LeaveWorkspaceCommand, bool>
{
    public async Task<bool> Handle(LeaveWorkspaceCommand request, CancellationToken cancellationToken)
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
        var currentMember = members.FirstOrDefault(m => m.UserId == currentUserId);
        
        if (currentMember == null)
        {
            throw new ArgumentException("You are not a member of this workspace");
        }

        var owners = members.Where(m => m.Role == WorkspaceRole.Owner).ToList();
        if (currentMember.Role == WorkspaceRole.Owner && owners.Count == 1)
        {
            var otherMembers = members.Where(m => m.UserId != currentUserId).ToList();
            if (otherMembers.Any())
            {
                throw new InvalidOperationException("Cannot leave workspace as the only owner. Please transfer ownership to another member first, or remove all other members before leaving.");
            }
        }

        await _workspaceRepository.RemoveMemberAsync(currentMember.Id);

        var memberLeftEvent = new MemberLeftEvent
        {
            UserId = currentUserId,
            WorkspaceId = request.WorkspaceId,
            LeftUserId = currentUserId,
            LeftUserName = currentMember.User?.Name ?? "Unknown User",
            LeftUserEmail = currentMember.User?.Email ?? "unknown@email.com",
            PreviousRole = currentMember.Role,
            WorkspaceName = workspace.Name,
            Description = $"{currentMember.User?.Name ?? "User"} left workspace '{workspace.Name}'",
            Metadata = $"{{\"MemberId\":\"{currentMember.Id}\",\"PreviousRole\":\"{currentMember.Role}\",\"LeftAt\":\"{DateTime.UtcNow:O}\"}}"
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
}