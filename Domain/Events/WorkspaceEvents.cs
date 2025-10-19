using Domain.Enums;

namespace Domain.Events;

public class WorkspaceCreatedEvent : ActivityDomainEvent
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string? WorkspaceDescription { get; set; }
    public string? WorkspaceLogo { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.WorkspaceCreated;
}

 public class WorkspaceUpdatedEvent : ActivityDomainEvent
{
    public Guid WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public string? WorkspaceDescription { get; set; }
    public string? WorkspaceLogo { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.WorkspaceUpdated;
}

public class WorkspaceDeletedEvent : ActivityDomainEvent
{
    public Guid WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.WorkspaceDeleted;
}
