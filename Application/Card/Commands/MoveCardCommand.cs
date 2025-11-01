using MediatR;
using Shared.DTOs;

namespace Application.Card.Commands;

public sealed record MoveCardCommand : IRequest<CardDto>
{
    public Guid CardId { get; set; }
    public Guid TargetListId { get; set; }
    public int? Position { get; set; }
}
