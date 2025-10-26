namespace Domain.Enums;

public enum ActivityType
{
    WorkspaceCreated, // done
    WorkspaceUpdated, // done
    WorkspaceDeleted, // done
    MemberInvited, // done
    MemberJoined, // done
    MemberRemoved, // done
    MemberLeft, // done
    MemberRoleChanged,
    BoardCreated, // done
    BoardUpdated, // done
    BoardDeleted, // done
    BoardMemberAssigned,
    BoardMemberUnassigned,
    ListCreated, // done
    ListUpdated, // done
    ListDeleted, // done
    ListMoved,
    CardCreated, // done
    CardUpdated, // done
    CardDeleted, // done
    CardMoved,
    CardMemberAssigned,
    CardMemberUnassigned,
    CardStatusChanged,
    CommentAdded,
    CommentUpdated,
    CommentDeleted
}