using MediatR;
using Shared.DTOs;

namespace Application.Workspace.Commands;

public sealed record CreateWorkspaceCommand : IRequest<WorkspaceDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
}
