using MediatR;
using Shared.DTOs;

namespace Application.Member.Queries;

public sealed record GetWorkspaceInvitationsQuery : IRequest<IEnumerable<InvitationDto>>
{
    public Guid WorkspaceId { get; set; }
}