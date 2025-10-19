namespace Domain.Events;

public class CommentCreatedEvent : ActivityDomainEvent
{
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public string CardTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool HasAttachments { get; set; }
    public int MentionCount { get; set; }
}

public class CommentUpdatedEvent : ActivityDomainEvent
{
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public string CardTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
}

public class CommentDeletedEvent : ActivityDomainEvent
{
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public string CardTitle { get; set; } = string.Empty;
    public bool WasSoftDeleted { get; set; }
}

public class CommentReactionAddedEvent : ActivityDomainEvent
{
    public Guid ReactionId { get; set; }
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public string CardTitle { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public Guid CommentAuthorId { get; set; }
}

public class UserMentionedInCommentEvent : ActivityDomainEvent
{
    public Guid MentionId { get; set; }
    public Guid CommentId { get; set; }
    public Guid CardId { get; set; }
    public string CardTitle { get; set; } = string.Empty;
    public Guid MentionedUserId { get; set; }
    public string MentionedUserName { get; set; } = string.Empty;
    public Guid CommentAuthorId { get; set; }
    public string CommentAuthorName { get; set; } = string.Empty;
    public string CommentContent { get; set; } = string.Empty;
}