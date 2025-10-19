namespace Shared.DTOs;

public class ListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public Guid BoardId { get; set; }
    public string BoardTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CardCount { get; set; }
    public IEnumerable<CardDto>? Cards { get; set; }
    public bool IsDeleted { get; set; }
}