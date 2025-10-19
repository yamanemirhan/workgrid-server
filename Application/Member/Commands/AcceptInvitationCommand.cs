using MediatR;
using Shared.DTOs;

namespace Application.Member.Commands;

public sealed record AcceptInvitationCommand : IRequest<MemberDto>
{
    public string Token { get; set; } = string.Empty;
}

public sealed record AcceptPublicInvitationCommand : IRequest<MemberDto>
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}