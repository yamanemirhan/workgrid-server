using Application.List.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.List.Handlers;

internal class CreateListHandler(IListRepository _listRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<CreateListCommand, ListDto>
{
    public async Task<ListDto> Handle(CreateListCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var board = await _boardRepository.GetByIdWithDetailsAsync(request.BoardId);
        if (board == null)
        {
            throw new ArgumentException("Board not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to create lists in this board");
        }

        var nextPosition = await _listRepository.GetNextPositionAsync(request.BoardId);

        var list = new Domain.Entities.List
        {
            Title = request.Title,
            BoardId = request.BoardId,
            CreatedBy = userId,
            Position = nextPosition
        };

        var createdList = await _listRepository.CreateAsync(list);

        var listCreatedEvent = new ListCreatedEvent
        {
            ListId = createdList.Id,
            BoardId = createdList.BoardId,
            WorkspaceId = board.WorkspaceId,
            UserId = userId,
            ListTitle = createdList.Title,
            Description = $"List created: {createdList.Title}",
            Metadata = null,
            ActivityType = ActivityType.ListCreated
        };
        await _rabbitMqPublisher.PublishAsync(listCreatedEvent);

        return new ListDto
        {
            Id = createdList.Id,
            Title = createdList.Title,
            Position = createdList.Position,
            BoardId = createdList.BoardId,
            BoardTitle = board.Title,
            CreatedAt = createdList.CreatedAt,
            CardCount = 0,
            Cards = new List<CardDto>()
        };
    }
}