namespace Domain.Entities;

public class BoardMember : BaseEntity
{
    public Guid BoardId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid AssignedBy { get; set; }

    // Navigation properties
    public Board Board { get; set; } = null!;
    public User User { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}