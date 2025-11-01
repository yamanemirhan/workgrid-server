using Application.CardStatus.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.CardStatus.Handlers;

internal class GetBoardCardStatusesHandler(ICardStatusRepository _cardStatusRepository,
        IListRepository _listRepository,
        IWorkspaceRepository _workspaceRepository,
        IBoardRepository _boardRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetBoardCardStatusesQuery, IEnumerable<CardStatusDto>>
{
    public async Task<IEnumerable<CardStatusDto>> Handle(GetBoardCardStatusesQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Verify board exists and get its workspace
        var board = await _boardRepository.GetByIdWithDetailsAsync(request.BoardId);
        if (board == null)
        {
            throw new ArgumentException("Board not found");
        }

        // Verify workspace access
        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        // Get all lists for this board (each list represents a status)
        var boardLists = await _listRepository.GetBoardListsAsync(request.BoardId);
        
        var statusDtos = new List<CardStatusDto>();
        
        foreach (var list in boardLists.OrderBy(l => l.Position))
        {
            if (list.StatusId.HasValue)
            {
                var status = await _cardStatusRepository.GetByIdAsync(list.StatusId.Value);
                if (status != null)
                {
                    // Count cards in this specific list
                    var cardCount = await _listRepository.GetCardCountByListIdAsync(list.Id);
                    
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
                        WorkspaceName = status.WorkspaceId.HasValue ? board.Workspace?.Name : "System Default",
                        CreatedAt = status.CreatedAt,
                        UpdatedAt = status.UpdatedAt,
                        CardCount = cardCount
                    });
                }
            }
        }

        return statusDtos;
    }
}