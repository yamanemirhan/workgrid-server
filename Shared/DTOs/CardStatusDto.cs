using Domain.Enums;

namespace Shared.DTOs;

public class CardStatusDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsDefault { get; set; }
    public CardStatusType Type { get; set; }
    public Guid? WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CardCount { get; set; }
}