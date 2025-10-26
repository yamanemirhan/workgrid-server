using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Workspace.Commands;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;

namespace Application.Workspace.Handlers;

internal class CreateWorkspaceHandler(IWorkspaceRepository _workspaceRepository,
     IUserRepository _userRepository,
     ICardStatusRepository _cardStatusRepository,
     IHttpContextAccessor _httpContextAccessor,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<CreateWorkspaceCommand, WorkspaceDto>
{
    public async Task<WorkspaceDto> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var workspace = new Domain.Entities.Workspace
        {
            Name = request.Name,
            Description = request.Description,
            Logo = request.Logo,
            OwnerId = userId
        };

        var createdWorkspace = await _workspaceRepository.CreateAsync(workspace);

        var ownerMember = new WorkspaceMember
        {
            UserId = userId,
            WorkspaceId = createdWorkspace.Id,
            Role = WorkspaceRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        await _workspaceRepository.AddMemberAsync(ownerMember);

        // Create default card statuses for the workspace (To-Do, In Progress, Done)
        await _cardStatusRepository.CreateDefaultStatusesForWorkspaceAsync(createdWorkspace.Id);

        var evt = new WorkspaceCreatedEvent
        {
            UserId = userId,
            WorkspaceId = createdWorkspace.Id,
            WorkspaceName = createdWorkspace.Name,
            WorkspaceDescription = createdWorkspace.Description,
            WorkspaceLogo = createdWorkspace.Logo,
            Description = $"Workspace '{createdWorkspace.Name}' created by {user.Name}",
            Metadata = null,
            ActivityType = ActivityType.WorkspaceCreated,
        };
        
        try
        {
            await _rabbitMqPublisher.PublishAsync(evt);
            Console.WriteLine($"[WorkspaceHandler] Successfully published WorkspaceCreatedEvent for workspace: {createdWorkspace.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WorkspaceHandler] WARNING: Failed to publish WorkspaceCreatedEvent for workspace {createdWorkspace.Name}: {ex.Message}");
        }

        return new WorkspaceDto
        {
            Id = createdWorkspace.Id,
            Name = createdWorkspace.Name,
            Description = createdWorkspace.Description,
            Logo = createdWorkspace.Logo,
            OwnerId = createdWorkspace.OwnerId,
            OwnerName = user.Name,
            CreatedAt = createdWorkspace.CreatedAt,
            MemberCount = 1,
            BoardCount = 0
        };
    }
}