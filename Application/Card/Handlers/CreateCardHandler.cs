using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Card.Commands;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using Domain.Enums;

namespace Application.Card.Handlers;

internal class CreateCardHandler(ICardRepository _cardRepository,
        IListRepository _listRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        ICardStatusRepository _cardStatusRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<CreateCardCommand, CardDto>
{
    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var list = await _listRepository.GetByIdWithDetailsAsync(request.ListId);
        if (list == null)
        {
            throw new ArgumentException("List not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, list.Board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to create cards in this list");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var todoStatus = await _cardStatusRepository.GetDefaultStatusByNameAsync("To-Do");
        if (todoStatus == null)
        {
            await _cardStatusRepository.CreateDefaultStatusesForWorkspaceAsync(list.Board.WorkspaceId);
            todoStatus = await _cardStatusRepository.GetDefaultStatusByNameAsync("To-Do");
        }

        var nextPosition = await _cardRepository.GetNextPositionAsync(request.ListId);

        var card = new Domain.Entities.Card
        {
            Title = request.Title,
            Description = request.Description,
            ListId = request.ListId,
            CreatedBy = userId,
            Position = nextPosition,
            EstimatedTime = request.EstimatedTime,
            Tags = request.Tags,
            StatusId = todoStatus?.Id
        };

        var createdCard = await _cardRepository.CreateAsync(card);

        var cardStatus = todoStatus != null ? new CardStatusDto
        {
            Id = todoStatus.Id,
            Name = todoStatus.Name,
            Description = todoStatus.Description,
            Color = todoStatus.Color,
            Position = todoStatus.Position,
            IsDefault = todoStatus.IsDefault,
            Type = todoStatus.Type,
            WorkspaceId = todoStatus.WorkspaceId,
            CreatedAt = todoStatus.CreatedAt,
            UpdatedAt = todoStatus.UpdatedAt
        } : null;

        var cardCreatedEvent = new CardCreatedEvent
        {
            CardId = createdCard.Id,
            BoardId = list.BoardId,
            ListId = createdCard.ListId,
            WorkspaceId = list.Board.WorkspaceId,
            UserId = createdCard.CreatedBy,
            CardTitle = createdCard.Title,
            Description = $"Card created: {createdCard.Title}",
            Metadata = null,
            ActivityType = ActivityType.CardCreated
        };
        await _rabbitMqPublisher.PublishAsync(cardCreatedEvent);

        return new CardDto
        {
            Id = createdCard.Id,
            Title = createdCard.Title,
            Description = createdCard.Description,
            Position = createdCard.Position,
            ListId = createdCard.ListId,
            ListTitle = list.Title,
            CreatedBy = createdCard.CreatedBy,
            CreatorName = user.Name,
            Creator = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            },
            EstimatedTime = createdCard.EstimatedTime,
            Tags = createdCard.Tags,
            StatusId = createdCard.StatusId,
            Status = cardStatus,
            CreatedAt = createdCard.CreatedAt
        };
    }
}