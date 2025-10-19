using MediatR;

namespace Application.Member.Commands;

public sealed record RemoveMemberCommand : IRequest<bool>
{
    public Guid WorkspaceId { get; set; }
    public Guid MemberId { get; set; }
}
