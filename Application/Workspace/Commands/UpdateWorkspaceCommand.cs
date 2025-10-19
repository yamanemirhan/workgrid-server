using MediatR;
using Shared.DTOs;

namespace Application.Workspace.Commands;

public sealed record UpdateWorkspaceCommand : IRequest<WorkspaceDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
}
