using Domain.Enums;

namespace Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime CurrentPeriodStart { get; set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodEnd { get; set; } = DateTime.UtcNow.AddMonths(1);
    
    // Navigation properties
    public Workspace Workspace { get; set; } = null!;
}