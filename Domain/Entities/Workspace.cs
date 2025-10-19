namespace Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<CardStatus> CardStatuses { get; set; } = new List<CardStatus>();
    public Subscription? Subscription { get; set; }
}
