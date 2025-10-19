using Domain.Enums;

namespace Domain.Entities;

public class CardStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#6B7280"; // Default gray color
    public int Position { get; set; }
    public bool IsDefault { get; set; } // Default statuses cannot be deleted or modified
    public CardStatusType Type { get; set; }
    public Guid? WorkspaceId { get; set; } // Null for default statuses, set for custom statuses
    
    // Navigation properties
    public Workspace? Workspace { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}