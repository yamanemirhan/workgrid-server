namespace Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<WorkspaceMember> WorkspaceMembers { get; set; } = new List<WorkspaceMember>();
    public ICollection<Workspace> OwnedWorkspaces { get; set; } = new List<Workspace>();
    public ICollection<Board> CreatedBoards { get; set; } = new List<Board>();
    public ICollection<Card> CreatedCards { get; set; } = new List<Card>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
    public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
}
