using MediatR;

namespace Application.Workspace.Commands;

public sealed record DeleteWorkspaceCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
