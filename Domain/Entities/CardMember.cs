namespace Domain.Entities;

public class CardMember : BaseEntity
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid AssignedBy { get; set; }

    // Navigation properties
    public Card Card { get; set; } = null!;
    public User User { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}