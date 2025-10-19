using MediatR;
using Shared.DTOs;
using Domain.Enums;

namespace Application.Member.Commands;

public sealed record InviteMemberCommand : IRequest<InvitationDto>
{
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
}
