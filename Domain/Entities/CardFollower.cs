namespace Domain.Entities;

public class CardFollower : BaseEntity
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Card Card { get; set; } = null!;
    public User User { get; set; } = null!;
}