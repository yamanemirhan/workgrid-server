using Application.Card.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;
using System.Text.Json;

namespace Application.Card.Handlers;

internal class MoveCardHandler(ICardRepository _cardRepository,
        IListRepository _listRepository,
        ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<MoveCardCommand, CardDto>
{
    public async Task<CardDto> Handle(MoveCardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.CardId);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        var targetList = await _listRepository.GetByIdWithDetailsAsync(request.TargetListId);
        if (targetList == null)
        {
            throw new ArgumentException("Target list not found");
        }

        // Check if card and target list are in the same board
        if (card.List.BoardId != targetList.BoardId)
        {
            throw new ArgumentException("Cannot move card to a list in a different board");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, card.List.Board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to move cards in this board");
        }

        // Check edit permissions
        var hasEditPermission = await _cardRepository.IsUserAuthorizedToEditCardAsync(userId, request.CardId);
        if (!hasEditPermission)
        {
            var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
            var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

            if (currentUserMember?.Role == WorkspaceRole.Member && card.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Members can only move cards they created themselves");
            }
        }

        var oldListId = card.ListId;
        var oldStatusId = card.StatusId;
        var isMovingToNewList = oldListId != request.TargetListId;

        // Update card's list and status to match the target list's status
        card.ListId = request.TargetListId;
        card.StatusId = targetList.StatusId;

        // Update position
        if (request.Position.HasValue)
        {
            card.Position = request.Position.Value;
        }
        else
        {
            var nextPosition = await _cardRepository.GetNextPositionAsync(request.TargetListId);
            card.Position = nextPosition;
        }

        card.UpdatedAt = DateTime.UtcNow;

        var updatedCard = await _cardRepository.UpdateAsync(card);

        if (isMovingToNewList)
        {
            await _cardRepository.ReorderCardsInListAsync(oldListId);
            
            await _cardRepository.ReorderCardsInListAsync(request.TargetListId, excludeCardId: updatedCard.Id);
            
            if (request.Position.HasValue)
            {
                var targetListCards = await _cardRepository.GetListCardsAsync(request.TargetListId);
                var cardsList = targetListCards.OrderBy(c => c.Position).ToList();
                
                for (int i = 0; i < cardsList.Count; i++)
                {
                    if (cardsList[i].Id == updatedCard.Id)
                    {
                        cardsList[i].Position = request.Position.Value;
                    }
                    else if (cardsList[i].Position >= request.Position.Value)
                    {
                        cardsList[i].Position = i + 1 + (i >= request.Position.Value - 1 ? 1 : 0);
                    }
                    else
                    {
                        cardsList[i].Position = i + 1;
                    }
                    cardsList[i].UpdatedAt = DateTime.UtcNow;
                }
                
                await _cardRepository.UpdateCardPositionsAsync(cardsList);
            }
        }
        else
        {
            if (request.Position.HasValue)
            {
                var listCards = await _cardRepository.GetListCardsAsync(request.TargetListId);
                var cardsList = listCards.OrderBy(c => c.Position).ToList();
                
                var oldPos = cardsList.FindIndex(c => c.Id == updatedCard.Id);
                var newPos = request.Position.Value - 1;
                
                if (oldPos != newPos && oldPos >= 0 && newPos >= 0 && newPos < cardsList.Count)
                {
                    var movedCard = cardsList[oldPos];
                    cardsList.RemoveAt(oldPos);
                    
                    cardsList.Insert(newPos, movedCard);
                    
                    // Update all positions
                    for (int i = 0; i < cardsList.Count; i++)
                    {
                        cardsList[i].Position = i + 1;
                        cardsList[i].UpdatedAt = DateTime.UtcNow;
                    }
                    
                    await _cardRepository.UpdateCardPositionsAsync(cardsList);
                }
            }
        }

        // Get the updated card with full details
        var cardWithDetails = await _cardRepository.GetByIdWithDetailsAsync(updatedCard.Id);

        CardStatusDto? statusDto = null;
        if (cardWithDetails!.StatusId.HasValue)
        {
            var status = await _cardStatusRepository.GetByIdAsync(cardWithDetails.StatusId.Value);
            if (status != null)
            {
                statusDto = new CardStatusDto
                {
                    Id = status.Id,
                    Name = status.Name,
                    Description = status.Description,
                    Color = status.Color,
                    Position = status.Position,
                    IsDefault = status.IsDefault,
                    Type = status.Type,
                    WorkspaceId = status.WorkspaceId,
                    CreatedAt = status.CreatedAt,
                    UpdatedAt = status.UpdatedAt
                };
            }
        }

        // Publish CardMoved event
        var cardMovedEvent = new CardMovedEvent
        {
            UserId = userId,
            WorkspaceId = card.List.Board.WorkspaceId,
            BoardId = card.List.BoardId,
            CardId = request.CardId,
            CardTitle = card.Title,
            OldListId = oldListId,
            NewListId = request.TargetListId,
            Description = $"Card '{card.Title}' moved from one list to another",
            ActivityType = ActivityType.CardMoved,
            Metadata = JsonSerializer.Serialize(new { 
                OldListId = oldListId,
                NewListId = request.TargetListId,
                OldStatusId = oldStatusId,
                NewStatusId = cardWithDetails.StatusId,
                CardTitle = card.Title
            })
        };
        await _rabbitMqPublisher.PublishAsync(cardMovedEvent);

        // If status changed, also publish CardStatusChanged event
        if (oldStatusId != cardWithDetails.StatusId)
        {
            var oldStatus = oldStatusId.HasValue ? await _cardStatusRepository.GetByIdAsync(oldStatusId.Value) : null;
            var newStatus = cardWithDetails.StatusId.HasValue ? await _cardStatusRepository.GetByIdAsync(cardWithDetails.StatusId.Value) : null;

            var statusChangedEvent = new CardStatusChangedEvent
            {
                UserId = userId,
                WorkspaceId = card.List.Board.WorkspaceId,
                BoardId = card.List.BoardId,
                ListId = cardWithDetails.ListId,
                CardId = request.CardId,
                CardTitle = card.Title,
                OldStatusId = oldStatusId,
                OldStatusName = oldStatus?.Name ?? "No Status",
                NewStatusId = cardWithDetails.StatusId ?? Guid.Empty,
                NewStatusName = newStatus?.Name ?? "No Status",
                Description = $"Card status changed from '{oldStatus?.Name ?? "No Status"}' to '{newStatus?.Name ?? "No Status"}' due to list change",
                ActivityType = ActivityType.CardStatusChanged,
                Metadata = JsonSerializer.Serialize(new { 
                    OldStatusId = oldStatusId,
                    OldStatusName = oldStatus?.Name ?? "No Status",
                    NewStatusId = cardWithDetails.StatusId,
                    NewStatusName = newStatus?.Name ?? "No Status",
                    CardTitle = card.Title,
                    Reason = "ListChange"
                })
            };
            await _rabbitMqPublisher.PublishAsync(statusChangedEvent);
        }

        return new CardDto
        {
            Id = cardWithDetails.Id,
            Title = cardWithDetails.Title,
            Description = cardWithDetails.Description,
            Position = cardWithDetails.Position,
            ListId = cardWithDetails.ListId,
            ListTitle = cardWithDetails.List?.Title ?? "Unknown",
            CreatedBy = cardWithDetails.CreatedBy,
            CreatorName = cardWithDetails.Creator?.Name ?? "Unknown",
            Creator = cardWithDetails.Creator != null ? new UserDto
            {
                Id = cardWithDetails.Creator.Id,
                Name = cardWithDetails.Creator.Name,
                Email = cardWithDetails.Creator.Email,
                Avatar = cardWithDetails.Creator.Avatar,
                CreatedAt = cardWithDetails.Creator.CreatedAt
            } : null,
            EstimatedTime = cardWithDetails.EstimatedTime,
            Tags = cardWithDetails.Tags,
            StatusId = cardWithDetails.StatusId,
            Status = statusDto,
            CreatedAt = cardWithDetails.CreatedAt,
            UpdatedAt = cardWithDetails.UpdatedAt,
            CardMembers = cardWithDetails.CardMembers?.Select(cm => new CardMemberDto
            {
                Id = cm.Id,
                CardId = cm.CardId,
                UserId = cm.UserId,
                UserName = cm.User?.Name ?? "Unknown",
                UserEmail = cm.User?.Email ?? "Unknown",
                UserAvatar = cm.User?.Avatar,
                AssignedAt = cm.AssignedAt,
                AssignedBy = cm.AssignedBy,
                AssignedByName = cm.AssignedByUser?.Name ?? "Unknown"
            }),
            CardFollowers = cardWithDetails.CardFollowers?.Select(cf => new CardFollowerDto
            {
                Id = cf.Id,
                CardId = cf.CardId,
                UserId = cf.UserId,
                UserName = cf.User?.Name ?? "Unknown",
                UserEmail = cf.User?.Email ?? "Unknown",
                UserAvatar = cf.User?.Avatar,
                FollowedAt = cf.FollowedAt
            })
        };
    }
}
