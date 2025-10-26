using Application.Board.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Board.Handlers;

internal class CreateBoardHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<CreateBoardCommand, BoardDto>
{
    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, request.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to create boards in this workspace");
        }

        var board = new Domain.Entities.Board
        {
            Title = request.Title,
            Description = request.Description,
            Logo = request.Logo,
            WorkspaceId = request.WorkspaceId,
            CreatedBy = userId,
            IsPrivate = request.IsPrivate
        };

        var createdBoard = await _boardRepository.CreateAsync(board);

        var boardCreatedEvent = new BoardCreatedEvent
        {
            BoardId = createdBoard.Id,
            BoardTitle = createdBoard.Title,
            WorkspaceId = createdBoard.WorkspaceId,
            UserId = createdBoard.CreatedBy,
            Description = $"Board created: {createdBoard.Title}",
            Metadata = null,
            ActivityType = ActivityType.BoardCreated,
        };
        
        try
        {
            await _rabbitMqPublisher.PublishAsync(boardCreatedEvent);
            Console.WriteLine($"[BoardHandler] Successfully published BoardCreatedEvent for board: {createdBoard.Title}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BoardHandler] WARNING: Failed to publish BoardCreatedEvent for board {createdBoard.Title}: {ex.Message}");

        }

        var workspaceWithDetails = await _workspaceRepository.GetByIdWithDetailsAsync(request.WorkspaceId);

        return new BoardDto
        {
            Id = createdBoard.Id,
            Title = createdBoard.Title,
            Description = createdBoard.Description,
            Logo = createdBoard.Logo,
            WorkspaceId = createdBoard.WorkspaceId,
            WorkspaceName = workspaceWithDetails?.Name ?? "Unknown",
            CreatedBy = createdBoard.CreatedBy,
            CreatorName = user.Name,
            CreatedAt = createdBoard.CreatedAt,
            ListCount = 0,
            CardCount = 0,
            IsPrivate = createdBoard.IsPrivate
        };
    }
}