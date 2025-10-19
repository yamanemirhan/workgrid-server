using MediatR;

namespace Application.Member.Commands;

public sealed record CancelInvitationCommand : IRequest<bool>
{
    public Guid InvitationId { get; set; }
}