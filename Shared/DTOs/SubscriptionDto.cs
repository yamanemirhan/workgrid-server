namespace Shared.DTOs;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}