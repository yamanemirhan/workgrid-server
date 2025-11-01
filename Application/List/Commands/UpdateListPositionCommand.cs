using MediatR;
using Shared.DTOs;

namespace Application.List.Commands;

public sealed record UpdateListPositionCommand : IRequest<ListDto>
{
    public Guid Id { get; set; }
    public int Position { get; set; }
}
