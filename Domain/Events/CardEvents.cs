using Domain.Enums;

namespace Domain.Events;

public class CardCreatedEvent : ActivityDomainEvent
{
    public Guid CardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string? CardTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.CardCreated;
}

public class CardUpdatedEvent : ActivityDomainEvent
{
    public Guid CardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string? CardTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.CardUpdated;
}

public class CardDeletedEvent : ActivityDomainEvent
{
    public Guid CardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string? CardTitle { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.CardDeleted;
}

public class CardMemberAssignedEvent : ActivityDomainEvent
{
    public Guid CardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string? CardTitle { get; set; }
    public Guid AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public string? AssignedUserEmail { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.CardMemberAssigned;
}

public class CardMemberUnassignedEvent : ActivityDomainEvent
{
    public Guid CardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string? CardTitle { get; set; }
    public Guid UnassignedUserId { get; set; }
    public string? UnassignedUserName { get; set; }
    public string? UnassignedUserEmail { get; set; }
    public ActivityType ActivityType { get; set; } = ActivityType.CardMemberUnassigned;
}

public class CommentAddedEvent : ActivityDomainEvent
{
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string? CardTitle { get; set; }
    public string? CommentContent { get; set; }
    public string? CommenterName { get; set; }
    public List<Guid> MentionedUserIds { get; set; } = new();
    public ActivityType ActivityType { get; set; } = ActivityType.CommentAdded;
}

public class CardStatusChangedEvent : ActivityDomainEvent
{
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public Guid CardId { get; set; }
    public string CardTitle { get; set; } = string.Empty;
    public Guid? OldStatusId { get; set; }
    public string OldStatusName { get; set; } = string.Empty;
    public Guid NewStatusId { get; set; }
    public string NewStatusName { get; set; } = string.Empty;
}
