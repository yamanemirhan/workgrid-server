using MediatR;
using Shared.DTOs;

namespace Application.Board.Commands;

public sealed record CreateBoardCommand : IRequest<BoardDto>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public Guid WorkspaceId { get; set; }
    public bool IsPrivate { get; set; } = false;
}
