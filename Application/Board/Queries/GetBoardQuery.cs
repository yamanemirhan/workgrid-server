using MediatR;
using Shared.DTOs;

namespace Application.Board.Queries;

public sealed record GetBoardQuery : IRequest<BoardDto?>
{
    public Guid Id { get; set; }
}
