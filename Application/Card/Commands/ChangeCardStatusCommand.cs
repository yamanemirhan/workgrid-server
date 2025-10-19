using MediatR;

namespace Application.Card.Commands;

public sealed record ChangeCardStatusCommand : IRequest<bool>
{
    public Guid CardId { get; set; }
    public Guid StatusId { get; set; }
}