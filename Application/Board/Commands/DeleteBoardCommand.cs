using MediatR;

namespace Application.Board.Commands;

public sealed record DeleteBoardCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
