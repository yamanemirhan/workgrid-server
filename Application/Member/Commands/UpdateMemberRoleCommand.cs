using MediatR;
using Shared.DTOs;
using Domain.Enums;

namespace Application.Member.Commands;

public sealed record UpdateMemberRoleCommand : IRequest<MemberDto>
{
    public Guid WorkspaceId { get; set; }
    public Guid MemberId { get; set; }
    public WorkspaceRole NewRole { get; set; }
}
