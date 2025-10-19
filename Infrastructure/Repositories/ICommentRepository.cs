using Domain.Entities;

namespace Infrastructure.Repositories;

public interface ICommentRepository
{
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetByCardIdAsync(Guid cardId, bool includeReplies = true);
    Task<Comment> UpdateAsync(Comment comment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<CommentReaction?> GetReactionAsync(Guid commentId, Guid userId);
    Task<CommentReaction> AddReactionAsync(CommentReaction reaction);
    Task<bool> RemoveReactionAsync(Guid commentId, Guid userId);
    Task<IEnumerable<CommentReaction>> GetCommentReactionsAsync(Guid commentId);
    Task<CommentMention> AddMentionAsync(CommentMention mention);
    Task<IEnumerable<CommentMention>> GetCommentMentionsAsync(Guid commentId);
    Task<CommentAttachment> AddAttachmentAsync(CommentAttachment attachment);
    Task<bool> RemoveAttachmentAsync(Guid attachmentId);
    Task<IEnumerable<CommentAttachment>> GetCommentAttachmentsAsync(Guid commentId);
    Task<bool> UserCanAccessCommentAsync(Guid userId, Guid commentId);
    Task<IEnumerable<Comment>> GetCommentHierarchyForCardAsync(Guid cardId); // New diagnostic method
}