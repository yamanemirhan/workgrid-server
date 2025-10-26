using Application.Card.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Card.Handlers;

internal class UpdateCardHandler(ICardRepository _cardRepository,
        ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UpdateCardCommand, CardDto>
{
    public async Task<CardDto> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.Id);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        // - Owners/Admins can edit any card
        // - Members can only edit cards they created themselves
        var hasEditPermission = await _cardRepository.IsUserAuthorizedToEditCardAsync(userId, request.Id);
        if (!hasEditPermission)
        {
            var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
            var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

            if (currentUserMember == null)
            {
                throw new UnauthorizedAccessException("You don't have access to this workspace");
            }

            if (currentUserMember.Role == WorkspaceRole.Member && card.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Members can only edit cards they created themselves");
            }

            throw new UnauthorizedAccessException("You don't have permission to update this card");
        }

        card.Title = request.Title;
        card.Description = request.Description;
        card.EstimatedTime = request.EstimatedTime;
        card.Tags = request.Tags;
        card.UpdatedAt = DateTime.UtcNow;

        var updatedCard = await _cardRepository.UpdateAsync(card);

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

        var cardUpdatedEvent = new CardUpdatedEvent
        {
            CardId = updatedCard.Id,
            BoardId = updatedCard.List?.BoardId ?? Guid.Empty,
            ListId = updatedCard.ListId,
            WorkspaceId = updatedCard.List?.Board?.WorkspaceId ?? Guid.Empty,
            UserId = userId,
            CardTitle = updatedCard.Title,
            Description = $"Card updated: {updatedCard.Title}",
            Metadata = null,
            ActivityType = ActivityType.CardUpdated
        };
        await _rabbitMqPublisher.PublishAsync(cardUpdatedEvent);

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
