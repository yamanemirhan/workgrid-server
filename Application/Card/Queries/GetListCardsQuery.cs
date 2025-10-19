using MediatR;
using Shared.DTOs;

namespace Application.Card.Queries;

public sealed record GetListCardsQuery : IRequest<IEnumerable<CardDto>>
{
    public Guid ListId { get; set; }
}