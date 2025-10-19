using Application.CardStatus.Commands;
using Domain.Enums;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.CardStatus.Handlers;

internal class UpdateCardStatusHandler(ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateCardStatusCommand, CardStatusDto>
{

    public async Task<CardStatusDto> Handle(UpdateCardStatusCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var cardStatus = await _cardStatusRepository.GetByIdAsync(request.Id);
        if (cardStatus == null)
        {
            throw new ArgumentException("Card status not found");
        }

        if (cardStatus.IsDefault)
        {
            throw new InvalidOperationException("Default statuses cannot be modified");
        }

        if (!cardStatus.WorkspaceId.HasValue)
        {
            throw new InvalidOperationException("Cannot update status without workspace");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(cardStatus.WorkspaceId.Value);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role != WorkspaceRole.Owner && currentUserMember.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace owners and admins can update custom card statuses");
        }

        var existingStatus = await _cardStatusRepository.GetByNameAndWorkspaceAsync(request.Name, cardStatus.WorkspaceId.Value);
        if (existingStatus != null && existingStatus.Id != request.Id)
        {
            throw new InvalidOperationException($"A status with the name '{request.Name}' already exists in this workspace");
        }

        cardStatus.Name = request.Name.Trim();
        cardStatus.Description = request.Description?.Trim();
        cardStatus.Color = request.Color;
        cardStatus.UpdatedAt = DateTime.UtcNow;

        var updatedStatus = await _cardStatusRepository.UpdateAsync(cardStatus);

        var workspace = await _workspaceRepository.GetByIdAsync(cardStatus.WorkspaceId.Value);
        var cardCount = await _cardStatusRepository.GetCardCountByStatusAsync(updatedStatus.Id);

        return new CardStatusDto
        {
            Id = updatedStatus.Id,
            Name = updatedStatus.Name,
            Description = updatedStatus.Description,
            Color = updatedStatus.Color,
            Position = updatedStatus.Position,
            IsDefault = updatedStatus.IsDefault,
            Type = updatedStatus.Type,
            WorkspaceId = updatedStatus.WorkspaceId,
            WorkspaceName = workspace?.Name,
            CreatedAt = updatedStatus.CreatedAt,
            UpdatedAt = updatedStatus.UpdatedAt,
            CardCount = cardCount
        };
    }
}