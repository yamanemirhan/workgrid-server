namespace Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "pending"; // pending, paid, failed
    public string? StripeInvoiceId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Navigation properties
    public Workspace Workspace { get; set; } = null!;
}
