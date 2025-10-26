using Application.CardStatus.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.CardStatus.Handlers;

internal class GetWorkspaceCardStatusesHandler(ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetWorkspaceCardStatusesQuery, IEnumerable<CardStatusDto>>
{
    public async Task<IEnumerable<CardStatusDto>> Handle(GetWorkspaceCardStatusesQuery request, CancellationToken cancellationToken)
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

        var statuses = await _cardStatusRepository.GetWorkspaceStatusesAsync(request.WorkspaceId);

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);

        var statusDtos = new List<CardStatusDto>();
        
        foreach (var status in statuses.OrderBy(s => s.Position))
        {
            var cardCount = await _cardStatusRepository.GetCardCountByStatusAndWorkspaceIdAsync(status.Id, workspace!.Id);
            
            statusDtos.Add(new CardStatusDto
            {
                Id = status.Id,
                Name = status.Name,
                Description = status.Description,
                Color = status.Color,
                Position = status.Position,
                IsDefault = status.IsDefault,
                Type = status.Type,
                WorkspaceId = status.WorkspaceId,
                WorkspaceName = status.WorkspaceId.HasValue ? workspace?.Name : "System Default",
                CreatedAt = status.CreatedAt,
                UpdatedAt = status.UpdatedAt,
                CardCount = cardCount
            });
        }

        return statusDtos;
    }
}