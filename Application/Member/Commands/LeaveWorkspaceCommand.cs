using MediatR;

namespace Application.Member.Commands;

public sealed record LeaveWorkspaceCommand : IRequest<bool>
{
    public Guid WorkspaceId { get; set; }
}