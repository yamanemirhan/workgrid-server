using Domain.Enums;

namespace Domain.Events;

public class ListCreatedEvent : ActivityDomainEvent
{
    public Guid ListId { get; set; }
    public Guid BoardId { get; set; }
    public string? ListTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.ListCreated;
}

public class ListUpdatedEvent : ActivityDomainEvent
{
    public Guid ListId { get; set; }
    public Guid BoardId { get; set; }
    public string? ListTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.ListUpdated;
}

public class ListDeletedEvent : ActivityDomainEvent
{
    public Guid ListId { get; set; }
    public Guid BoardId { get; set; }
    public string? ListTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.ListDeleted;
}
