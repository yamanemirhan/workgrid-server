using Application.CardStatus.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.CardStatus.Handlers;

internal class GetCardStatusByIdHandler(ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetCardStatusByIdQuery, CardStatusDto?>
{
    public async Task<CardStatusDto?> Handle(GetCardStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var cardStatus = await _cardStatusRepository.GetByIdAsync(request.Id);
        if (cardStatus == null)
        {
            return null;
        }

        if (cardStatus.WorkspaceId.HasValue)
        {
            var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(cardStatus.WorkspaceId.Value);
            var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

            if (currentUserMember == null)
            {
                throw new UnauthorizedAccessException("You don't have access to this workspace");
            }
        }

        var workspace = cardStatus.WorkspaceId.HasValue 
            ? await _workspaceRepository.GetByIdAsync(cardStatus.WorkspaceId.Value) 
            : null;
        var cardCount = await _cardStatusRepository.GetCardCountByStatusAsync(cardStatus.Id);

        return new CardStatusDto
        {
            Id = cardStatus.Id,
            Name = cardStatus.Name,
            Description = cardStatus.Description,
            Color = cardStatus.Color,
            Position = cardStatus.Position,
            IsDefault = cardStatus.IsDefault,
            Type = cardStatus.Type,
            WorkspaceId = cardStatus.WorkspaceId,
            WorkspaceName = cardStatus.WorkspaceId.HasValue ? workspace?.Name : "System Default",
            CreatedAt = cardStatus.CreatedAt,
            UpdatedAt = cardStatus.UpdatedAt,
            CardCount = cardCount
        };
    }
}