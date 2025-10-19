using MediatR;

namespace Application.Board.Commands;

public sealed record UnassignUserFromBoardCommand : IRequest<bool>
{
    public Guid BoardId { get; set; }
    public Guid UserId { get; set; }
}