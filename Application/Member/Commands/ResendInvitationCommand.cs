using MediatR;
using Shared.DTOs;

namespace Application.Member.Commands;

public sealed record ResendInvitationCommand : IRequest<InvitationDto>
{
    public Guid InvitationId { get; set; }
}