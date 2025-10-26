using Domain.Enums;

namespace Domain.Events;

public class MemberInvitedEvent : ActivityDomainEvent
{
    public Guid InvitedUserId { get; set; }
    public Guid InvitedBy { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public string? InvitedUserName { get; set; }
    public string InviterName { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
    public string Token { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; } = ActivityType.MemberInvited;
}

public class MemberJoinedEvent : ActivityDomainEvent
{
    public Guid JoinedUserId { get; set; }
    public string JoinedUserName { get; set; } = string.Empty;
    public string JoinedUserEmail { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; } = ActivityType.MemberJoined;
}

public class MemberRemovedEvent : ActivityDomainEvent
{
    public Guid RemovedUserId { get; set; }
    public string RemovedUserName { get; set; } = string.Empty;
    public string RemovedUserEmail { get; set; } = string.Empty;
    public Guid RemovedBy { get; set; }
    public string RemovedByName { get; set; } = string.Empty;
    public WorkspaceRole PreviousRole { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; } = ActivityType.MemberRemoved;
}

public class MemberLeftEvent : ActivityDomainEvent
{
    public Guid LeftUserId { get; set; }
    public string LeftUserName { get; set; } = string.Empty;
    public string LeftUserEmail { get; set; } = string.Empty;
    public WorkspaceRole PreviousRole { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; } = ActivityType.MemberRemoved;
}

public class MemberRoleChangedEvent : ActivityDomainEvent
{
    public Guid TargetUserId { get; set; }
    public string TargetUserName { get; set; } = string.Empty;
    public string TargetUserEmail { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public WorkspaceRole OldRole { get; set; }
    public WorkspaceRole NewRole { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public ActivityType ActivityType { get; set; } = ActivityType.MemberRoleChanged;
}
