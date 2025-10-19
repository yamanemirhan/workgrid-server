using Application.CardStatus.Commands;
using Domain.Enums;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.CardStatus.Handlers;

internal class CreateCardStatusHandler(ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CreateCardStatusCommand, CardStatusDto>
{
    public async Task<CardStatusDto> Handle(CreateCardStatusCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(request.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role != WorkspaceRole.Owner && currentUserMember.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace owners and admins can create custom card statuses");
        }

        var existingStatus = await _cardStatusRepository.GetByNameAndWorkspaceAsync(request.Name, request.WorkspaceId);
        if (existingStatus != null)
        {
            throw new InvalidOperationException($"A status with the name '{request.Name}' already exists in this workspace");
        }

        // Get existing statuses to determine position
        var existingStatuses = await _cardStatusRepository.GetWorkspaceStatusesAsync(request.WorkspaceId);
        var maxPosition = existingStatuses.Any() ? existingStatuses.Max(s => s.Position) : 0;

        var cardStatus = new Domain.Entities.CardStatus
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Color = request.Color,
            Position = maxPosition + 1,
            IsDefault = false,
            Type = CardStatusType.Custom,
            WorkspaceId = request.WorkspaceId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdStatus = await _cardStatusRepository.CreateAsync(cardStatus);

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);

        return new CardStatusDto
        {
            Id = createdStatus.Id,
            Name = createdStatus.Name,
            Description = createdStatus.Description,
            Color = createdStatus.Color,
            Position = createdStatus.Position,
            IsDefault = createdStatus.IsDefault,
            Type = createdStatus.Type,
            WorkspaceId = createdStatus.WorkspaceId,
            WorkspaceName = workspace?.Name,
            CreatedAt = createdStatus.CreatedAt,
            UpdatedAt = createdStatus.UpdatedAt,
            CardCount = 0 // New status has no cards yet
        };
    }
}