using MediatR;
using Shared.DTOs;

namespace Application.Member.Queries;

public sealed record GetWorkspaceMembersQuery : IRequest<IEnumerable<MemberDto>>
{
    public Guid WorkspaceId { get; set; }
}