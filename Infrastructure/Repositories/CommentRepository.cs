using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CommentRepository(AppDbContext _context
    ) : ICommentRepository
{
    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Attachments)
            .Include(c => c.Mentions)
                .ThenInclude(m => m.MentionedUser)
            .Include(c => c.Reactions)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<Comment>> GetByCardIdAsync(Guid cardId, bool includeReplies = true)
    {
        // Return all comments for the card, ordered by newest first
        return await _context.Comments
            .Where(c => c.CardId == cardId && !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Attachments)
            .Include(c => c.Mentions)
                .ThenInclude(m => m.MentionedUser)
            .Include(c => c.Reactions)
                .ThenInclude(r => r.User)
            .OrderByDescending(c => c.CreatedAt) // Newest first
            .ToListAsync();
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return false;

        _context.Comments.Remove(comment);
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return false;

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CommentReaction?> GetReactionAsync(Guid commentId, Guid userId)
    {
        return await _context.CommentReactions
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);
    }

    public async Task<CommentReaction> AddReactionAsync(CommentReaction reaction)
    {
        // Remove existing reaction first (user can only have one reaction per comment)
        var existingReaction = await GetReactionAsync(reaction.CommentId, reaction.UserId);
        if (existingReaction != null)
        {
            _context.CommentReactions.Remove(existingReaction);
        }

        _context.CommentReactions.Add(reaction);
        await _context.SaveChangesAsync();
        return reaction;
    }

    public async Task<bool> RemoveReactionAsync(Guid commentId, Guid userId)
    {
        var reaction = await GetReactionAsync(commentId, userId);
        if (reaction == null) return false;

        _context.CommentReactions.Remove(reaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CommentReaction>> GetCommentReactionsAsync(Guid commentId)
    {
        return await _context.CommentReactions
            .Where(r => r.CommentId == commentId)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<CommentMention> AddMentionAsync(CommentMention mention)
    {
        _context.CommentMentions.Add(mention);
        await _context.SaveChangesAsync();
        return mention;
    }

    public async Task<IEnumerable<CommentMention>> GetCommentMentionsAsync(Guid commentId)
    {
        return await _context.CommentMentions
            .Where(m => m.CommentId == commentId)
            .Include(m => m.MentionedUser)
            .ToListAsync();
    }

    public async Task<CommentAttachment> AddAttachmentAsync(CommentAttachment attachment)
    {
        _context.CommentAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<bool> RemoveAttachmentAsync(Guid attachmentId)
    {
        var attachment = await _context.CommentAttachments.FindAsync(attachmentId);
        if (attachment == null) return false;

        _context.CommentAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CommentAttachment>> GetCommentAttachmentsAsync(Guid commentId)
    {
        return await _context.CommentAttachments
            .Where(a => a.CommentId == commentId)
            .ToListAsync();
    }

    public async Task<bool> UserCanAccessCommentAsync(Guid userId, Guid commentId)
    {
        return await _context.Comments
            .Where(c => c.Id == commentId)
            .Join(_context.Cards, c => c.CardId, card => card.Id, (c, card) => new { c, card })
            .Join(_context.Lists, cc => cc.card.ListId, list => list.Id, (cc, list) => new { cc.c, cc.card, list })
            .Join(_context.Boards, ccl => ccl.list.BoardId, board => board.Id, (ccl, board) => new { ccl.c, ccl.card, ccl.list, board })
            .Join(_context.Workspaces, cclb => cclb.board.WorkspaceId, workspace => workspace.Id, (cclb, workspace) => new { cclb, workspace })
            .AnyAsync(x => _context.WorkspaceMembers.Any(wm => 
                wm.WorkspaceId == x.workspace.Id && wm.UserId == userId));
    }

    public async Task<IEnumerable<Comment>> GetCommentHierarchyForCardAsync(Guid cardId)
    {
        // Return all comments for a card for diagnostic purposes
        return await _context.Comments
            .Where(c => c.CardId == cardId && !c.IsDeleted)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}