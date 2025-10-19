using MediatR;
using Shared.DTOs;

namespace Application.Board.Commands;

public sealed record UpdateBoardCommand : IRequest<BoardDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public bool IsPrivate { get; set; }
}
