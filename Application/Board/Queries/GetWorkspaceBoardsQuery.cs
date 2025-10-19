using MediatR;
using Shared.DTOs;

namespace Application.Board.Queries;

public sealed record GetWorkspaceBoardsQuery : IRequest<IEnumerable<BoardDto>>
{
    public Guid WorkspaceId { get; set; }
}