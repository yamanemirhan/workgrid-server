namespace Shared.DTOs;

public class BoardDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ListCount { get; set; }
    public int CardCount { get; set; }
    public bool IsPrivate { get; set; }
    public IEnumerable<ListDto>? Lists { get; set; }
    public IEnumerable<MemberDto>? BoardMembers { get; set; }
    public bool IsDeleted { get; set; }
}
