using Domain.Entities;

namespace Infrastructure.Repositories;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id);
    Task<Card?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Card>> GetListCardsAsync(Guid listId);
    Task<Card> CreateAsync(Card card);
    Task<Card> UpdateAsync(Card card);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetNextPositionAsync(Guid listId);
    Task<bool> IsUserAuthorizedToAccessCardAsync(Guid userId, Guid cardId);
    Task<bool> IsUserAuthorizedToEditCardAsync(Guid userId, Guid cardId);
    
    Task ReorderCardsInListAsync(Guid listId, Guid? excludeCardId = null);
    Task UpdateCardPositionsAsync(IEnumerable<Card> cards);
    
    // Card Member methods
    Task<CardMember> AssignUserToCardAsync(CardMember cardMember);
    Task<bool> UnassignUserFromCardAsync(Guid cardId, Guid userId);
    Task<IEnumerable<CardMember>> GetCardMembersAsync(Guid cardId);
    Task<bool> IsUserCardMemberAsync(Guid cardId, Guid userId);
    
    // Card Follower methods
    Task<CardFollower> FollowCardAsync(CardFollower cardFollower);
    Task<bool> UnfollowCardAsync(Guid cardId, Guid userId);
    Task<IEnumerable<CardFollower>> GetCardFollowersAsync(Guid cardId);
    Task<bool> IsUserFollowingCardAsync(Guid cardId, Guid userId);
}