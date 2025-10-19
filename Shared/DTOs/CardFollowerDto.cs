namespace Shared.DTOs;

public class CardFollowerDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime FollowedAt { get; set; }
}