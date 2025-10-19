using MediatR;
using Shared.DTOs;

namespace Application.Card.Commands;

public sealed record AssignUserToCardCommand : IRequest<CardMemberDto>
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
}