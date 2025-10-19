namespace Domain.Entities;

public class Board : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPrivate { get; set; } = false;

    // Navigation properties
    public Workspace Workspace { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<List> Lists { get; set; } = new List<List>();
    public ICollection<BoardMember> BoardMembers { get; set; } = new List<BoardMember>();
}
