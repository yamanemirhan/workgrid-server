using MediatR;
using Shared.DTOs;

namespace Application.Card.Commands;

public sealed record FollowCardCommand : IRequest<CardFollowerDto>
{
    public Guid CardId { get; set; }
}