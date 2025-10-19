using MediatR;
using Shared.DTOs;

namespace Application.Workspace.Queries;

public sealed record GetWorkspaceQuery : IRequest<WorkspaceDto?>
{
    public Guid Id { get; set; }
}
