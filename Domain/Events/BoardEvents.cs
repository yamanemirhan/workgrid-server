using Domain.Enums;

namespace Domain.Events;

public class BoardCreatedEvent : ActivityDomainEvent
{
    public Guid BoardId { get; set; }
    public string? BoardTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.BoardCreated;
}

public class BoardUpdatedEvent : ActivityDomainEvent
{
    public Guid BoardId { get; set; }
    public string? BoardTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.BoardUpdated;
}

public class BoardDeletedEvent : ActivityDomainEvent
{
    public Guid BoardId { get; set; }
    public string? BoardTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.BoardDeleted;
}

public class BoardMemberAssignedEvent : ActivityDomainEvent
{
    public Guid BoardId { get; set; }
    public string? BoardTitle { get; set; }
    public Guid AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserEmail { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.BoardMemberAssigned;
}

public class BoardMemberUnassignedEvent : ActivityDomainEvent
{
    public Guid BoardId { get; set; }
    public string? BoardTitle { get; set; }
    public Guid UnassignedUserId { get; set; }
    public string? UnassignedUserName { get; set; }
    public string? UnassignedUserEmail { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.BoardMemberUnassigned;
}
