namespace Shared.DTOs;

public class CardMemberDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid AssignedBy { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
}