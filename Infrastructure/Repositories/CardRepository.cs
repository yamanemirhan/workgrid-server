using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardRepository(AppDbContext _context) : ICardRepository
{
    public async Task<Card?> GetByIdAsync(Guid id)
    {
        return await _context.Cards
            .Include(c => c.List)
                .ThenInclude(l => l.Board)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Card?> GetByIdWithDetailsAsync(Guid id)
    {
        var card = await _context.Cards
            .Include(c => c.List)
                .ThenInclude(l => l.Board)
                    .ThenInclude(b => b.Workspace)
            .Include(c => c.Creator)
            .Include(c => c.Status)
            .Include(c => c.CardMembers)
                .ThenInclude(cm => cm.User)
            .Include(c => c.CardMembers)
                .ThenInclude(cm => cm.AssignedByUser)
            .Include(c => c.CardFollowers)
                .ThenInclude(cf => cf.User)
            .Include(c => c.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
            .Include(c => c.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Reactions)
                    .ThenInclude(r => r.User)
            .Include(c => c.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Attachments)
            .Include(c => c.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Mentions)
                    .ThenInclude(m => m.MentionedUser)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id
                                      && !c.IsDeleted
                                      && !c.List.IsDeleted
                                      && !c.List.Board.IsDeleted
                                      && !c.List.Board.Workspace.IsDeleted);

        if (card == null)
            return null;

        card.Comments = card.Comments
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return card;
    }

    public async Task<IEnumerable<Card>> GetListCardsAsync(Guid listId)
    {
        return await _context.Cards
            .Include(c => c.List)
            .Include(c => c.Creator)
            .Include(c => c.CardMembers)
                .ThenInclude(cm => cm.User)
            .Include(c => c.CardFollowers)
                .ThenInclude(cf => cf.User)
            .Include(c => c.Status)
            .Where(c => c.ListId == listId
                        && !c.IsDeleted
                        && !c.List.IsDeleted
                        && !c.List.Board.IsDeleted
                        && !c.List.Board.Workspace.IsDeleted)
            .OrderBy(c => c.Position)
            .ToListAsync();
    }

    public async Task<Card> CreateAsync(Card card)
    {
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();
        return card;
    }

    public async Task<Card> UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
        return card;
    }

    public async Task DeleteAsync(Guid id)
    {
        var card = await _context.Cards.FirstOrDefaultAsync(c => c.Id == id);
        if (card != null)
        {
            card.IsDeleted = true;
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Cards
            .AnyAsync(c => c.Id == id
                           && !c.IsDeleted
                           && !c.List.IsDeleted
                           && !c.List.Board.IsDeleted
                           && !c.List.Board.Workspace.IsDeleted);
    }

    public async Task<int> GetNextPositionAsync(Guid listId)
    {
        var maxPosition = await _context.Cards
            .Where(c => c.ListId == listId
                        && !c.IsDeleted
                        && !c.List.IsDeleted
                        && !c.List.Board.IsDeleted
                        && !c.List.Board.Workspace.IsDeleted)
            .MaxAsync(c => (int?)c.Position);

        return (maxPosition ?? 0) + 1;
    }

    public async Task<bool> IsUserAuthorizedToAccessCardAsync(Guid userId, Guid cardId)
    {
        return await _context.Cards
            .Include(c => c.List)
                .ThenInclude(l => l.Board)
                    .ThenInclude(b => b.Workspace)
            .AnyAsync(c => c.Id == cardId
                           && !c.IsDeleted
                           && !c.List.IsDeleted
                           && !c.List.Board.IsDeleted
                           && !c.List.Board.Workspace.IsDeleted
                           && (c.List.Board.Workspace.OwnerId == userId
                               || c.List.Board.Workspace.Members.Any(m => m.UserId == userId)));
    }

    public async Task<bool> IsUserAuthorizedToEditCardAsync(Guid userId, Guid cardId)
    {
        return await _context.Cards
            .Include(c => c.List)
                .ThenInclude(l => l.Board)
                    .ThenInclude(b => b.Workspace)
                        .ThenInclude(w => w.Members)
            .Where(c => c.Id == cardId
                       && !c.IsDeleted
                       && !c.List.IsDeleted
                       && !c.List.Board.IsDeleted
                       && !c.List.Board.Workspace.IsDeleted)
            .AnyAsync(c => 
                // Workspace Owner can always edit
                c.List.Board.Workspace.OwnerId == userId ||
                // Workspace Admin can always edit
                c.List.Board.Workspace.Members.Any(m => m.UserId == userId && m.Role == Domain.Enums.WorkspaceRole.Admin) ||
                // Member can only edit cards they created
                (c.List.Board.Workspace.Members.Any(m => m.UserId == userId && m.Role == Domain.Enums.WorkspaceRole.Member) && c.CreatedBy == userId));
    }

    // Card Member methods
    public async Task<CardMember> AssignUserToCardAsync(CardMember cardMember)
    {
        _context.CardMembers.Add(cardMember);
        await _context.SaveChangesAsync();
        
        return await _context.CardMembers
            .Include(cm => cm.User)
            .Include(cm => cm.AssignedByUser)
            .FirstAsync(cm => cm.Id == cardMember.Id);
    }

    public async Task<bool> UnassignUserFromCardAsync(Guid cardId, Guid userId)
    {
        var cardMember = await _context.CardMembers
            .FirstOrDefaultAsync(cm => cm.CardId == cardId && cm.UserId == userId);
        
        if (cardMember == null) return false;
        
        _context.CardMembers.Remove(cardMember);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CardMember>> GetCardMembersAsync(Guid cardId)
    {
        return await _context.CardMembers
            .Include(cm => cm.User)
            .Include(cm => cm.AssignedByUser)
            .Where(cm => cm.CardId == cardId)
            .ToListAsync();
    }

    public async Task<bool> IsUserCardMemberAsync(Guid cardId, Guid userId)
    {
        return await _context.CardMembers
            .AnyAsync(cm => cm.CardId == cardId && cm.UserId == userId);
    }

    // Card Follower methods
    public async Task<CardFollower> FollowCardAsync(CardFollower cardFollower)
    {
        _context.CardFollowers.Add(cardFollower);
        await _context.SaveChangesAsync();
        
        return await _context.CardFollowers
            .Include(cf => cf.User)
            .FirstAsync(cf => cf.Id == cardFollower.Id);
    }

    public async Task<bool> UnfollowCardAsync(Guid cardId, Guid userId)
    {
        var cardFollower = await _context.CardFollowers
            .FirstOrDefaultAsync(cf => cf.CardId == cardId && cf.UserId == userId);
        
        if (cardFollower == null) return false;
        
        _context.CardFollowers.Remove(cardFollower);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CardFollower>> GetCardFollowersAsync(Guid cardId)
    {
        return await _context.CardFollowers
            .Include(cf => cf.User)
            .Where(cf => cf.CardId == cardId)
            .ToListAsync();
    }

    public async Task<bool> IsUserFollowingCardAsync(Guid cardId, Guid userId)
    {
        return await _context.CardFollowers
            .AnyAsync(cf => cf.CardId == cardId && cf.UserId == userId);
    }

    public async Task ReorderCardsInListAsync(Guid listId, Guid? excludeCardId = null)
    {
        var cards = await _context.Cards
            .Where(c => c.ListId == listId
                        && !c.IsDeleted
                        && (!excludeCardId.HasValue || c.Id != excludeCardId.Value))
            .OrderBy(c => c.Position)
            .ToListAsync();

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].Position = i + 1;
            cards[i].UpdatedAt = DateTime.UtcNow;
        }

        _context.Cards.UpdateRange(cards);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCardPositionsAsync(IEnumerable<Card> cards)
    {
        _context.Cards.UpdateRange(cards);
        await _context.SaveChangesAsync();
    }
}