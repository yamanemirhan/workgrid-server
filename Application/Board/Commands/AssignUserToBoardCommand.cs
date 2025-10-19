using MediatR;
using Shared.DTOs;

namespace Application.Board.Commands;

public sealed record AssignUserToBoardCommand : IRequest<MemberDto>
{
    public Guid BoardId { get; set; }
    public Guid UserId { get; set; }
}