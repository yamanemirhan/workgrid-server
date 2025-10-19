namespace Shared.DTOs;

public class WorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int BoardCount { get; set; }
    public SubscriptionDto? Subscription { get; set; }
    public bool IsDeleted { get; set; }
}
