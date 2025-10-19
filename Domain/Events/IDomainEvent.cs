namespace Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public abstract class ActivityDomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; }
}
