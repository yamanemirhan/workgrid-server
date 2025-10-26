using Application.Workspace.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Workspace.Handlers;

internal class DeleteWorkspaceHandler(IWorkspaceRepository _workspaceRepository,
    IHttpContextAccessor _httpContextAccessor,
    IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<DeleteWorkspaceCommand, bool>
{
    public async Task<bool> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("Only workspace owner can delete the workspace");
        }

        await _workspaceRepository.DeleteAsync(request.Id);

        var workspaceDeletedEvent = new WorkspaceDeletedEvent
        {
            WorkspaceId = workspace.Id,
            UserId = userId,
            WorkspaceName = workspace.Name,
            Description = $"Workspace deleted: {workspace.Name}",
            Metadata = null,
            ActivityType = ActivityType.WorkspaceDeleted
        };
        await _rabbitMqPublisher.PublishAsync(workspaceDeletedEvent);

        return true;
    }
}