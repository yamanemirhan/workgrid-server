namespace Domain.Enums;

public enum NotificationType
{
    // Workspace notifications
    WorkspaceInvitation,
    WorkspaceJoined,
    WorkspaceLeft,
    WorkspaceCreated,
    WorkspaceUpdated,
    
    // Member notifications
    MemberInvited,
    MemberJoined,
    MemberRemoved,
    MemberLeft,
    MemberRoleChanged,
    
    // Board notifications
    BoardCreated,
    BoardUpdated,
    BoardDeleted,
    BoardMemberAdded,
    BoardMemberAssigned,
    BoardMemberUnassigned,
    
    // List notifications
    ListCreated,
    ListUpdated,
    ListDeleted,
    ListMoved,
    
    // Card notifications
    CardCreated,
    CardUpdated,
    CardDeleted,
    CardMoved,
    CardAssigned,
    CardMemberAssigned,
    CardMemberUnassigned,
    CardCommentAdded,
    
    // Comment notifications
    CommentAdded,
    CommentReplied,
    CommentMentioned,
    CommentUpdated,
    CommentDeleted,
    
    // System notifications
    SystemMaintenance,
    SystemUpdate
}