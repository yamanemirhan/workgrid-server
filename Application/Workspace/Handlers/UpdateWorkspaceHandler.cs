using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Workspace.Commands;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;

namespace Application.Workspace.Handlers;

internal class UpdateWorkspaceHandler(IWorkspaceRepository _workspaceRepository,
     IUserRepository _userRepository,
     IHttpContextAccessor _httpContextAccessor,
     IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UpdateWorkspaceCommand, WorkspaceDto>
{    
    public async Task<WorkspaceDto> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.Id);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found");
        }

        if (workspace.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only workspace owner can update the workspace");
        }

        workspace.Name = request.Name;
        workspace.Description = request.Description;
        workspace.Logo = request.Logo;
        workspace.UpdatedAt = DateTime.UtcNow;

        var updatedWorkspace = await _workspaceRepository.UpdateAsync(workspace);

        var workspaceUpdatedEvent = new WorkspaceUpdatedEvent
        {
            WorkspaceId = updatedWorkspace.Id,
            UserId = userId,
            WorkspaceName = updatedWorkspace.Name,
            WorkspaceDescription = updatedWorkspace.Description,
            WorkspaceLogo = updatedWorkspace.Logo,
            Description = $"Workspace updated: {updatedWorkspace.Name}",
            Metadata = null
        };
        await _rabbitMqPublisher.PublishAsync(workspaceUpdatedEvent);

        var workspaceWithDetails = await _workspaceRepository.GetByIdWithDetailsAsync(updatedWorkspace.Id);
        var owner = await _userRepository.GetByIdAsync(workspaceWithDetails.OwnerId);

        return new WorkspaceDto
        {
            Id = workspaceWithDetails.Id,
            Name = workspaceWithDetails.Name,
            Description = workspaceWithDetails.Description,
            Logo = workspaceWithDetails.Logo,
            OwnerId = workspaceWithDetails.OwnerId,
            OwnerName = owner?.Name ?? "Unknown",
            CreatedAt = workspaceWithDetails.CreatedAt,
            MemberCount = workspaceWithDetails.Members?.Count() ?? 0,
            BoardCount = workspaceWithDetails.Boards?.Count() ?? 0
        };
    }
}