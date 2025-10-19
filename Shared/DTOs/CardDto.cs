namespace Shared.DTOs;

public class CardDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public Guid ListId { get; set; }
    public string ListTitle { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public UserDto? Creator { get; set; }
    public string? EstimatedTime { get; set; }
    public string? Tags { get; set; }
    public Guid? StatusId { get; set; }
    public CardStatusDto? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public IEnumerable<CardMemberDto>? CardMembers { get; set; }
    public IEnumerable<CardFollowerDto>? CardFollowers { get; set; }
    public IEnumerable<CommentDto>? CardComments { get; set; }
}
