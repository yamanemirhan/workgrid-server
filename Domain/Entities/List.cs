namespace Domain.Entities;

public class List : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public Guid BoardId { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDeleted { get; set; } // Soft delete

    // Navigation properties
    public Board Board { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}