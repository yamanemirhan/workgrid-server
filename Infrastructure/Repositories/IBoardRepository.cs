using Domain.Entities;

namespace Infrastructure.Repositories;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(Guid id);
    Task<Board?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Board>> GetWorkspaceBoardsAsync(Guid workspaceId);
    Task<IEnumerable<Board>> GetWorkspaceBoardsForUserAsync(Guid workspaceId, Guid userId);
    Task<Board> CreateAsync(Board board);
    Task<Board> UpdateAsync(Board board);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCardCountByBoardIdAsync(Guid boardId);
    
    // Board Member methods
    Task<BoardMember> AssignUserToBoardAsync(BoardMember boardMember);
    Task<bool> UnassignUserFromBoardAsync(Guid boardId, Guid userId);
    Task<IEnumerable<BoardMember>> GetBoardMembersAsync(Guid boardId);
    Task<bool> IsUserBoardMemberAsync(Guid boardId, Guid userId);
}