using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BoardRepository(AppDbContext _context) : IBoardRepository
{
    public async Task<Board?> GetByIdAsync(Guid id)
    {
        return await _context.Boards.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    public async Task<Board?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Boards
            .Include(b => b.Workspace)
            .Include(b => b.Creator)
            .Include(b => b.BoardMembers)
                .ThenInclude(bm => bm.User)
            .Include(b => b.Lists
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Position))
                .ThenInclude(l => l.Cards
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Position))
                    .ThenInclude(c => c.Creator)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Cards)
                    .ThenInclude(c => c.CardMembers)
                        .ThenInclude(cm => cm.User)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Cards)
                    .ThenInclude(c => c.CardMembers)
                        .ThenInclude(cm => cm.AssignedByUser)
            .Include(b => b.Lists)
                .ThenInclude(l => l.Cards)
                    .ThenInclude(c => c.CardFollowers)
                        .ThenInclude(cf => cf.User)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted && !b.Workspace.IsDeleted);
    }

    public async Task<IEnumerable<Board>> GetWorkspaceBoardsAsync(Guid workspaceId)
    {
        return await _context.Boards
            .Include(b => b.Creator)
            .Include(b => b.BoardMembers)
                .ThenInclude(bm => bm.User)
            .Include(b => b.Lists
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Position))
                .ThenInclude(l => l.Cards
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Position))
            .Where(b => b.WorkspaceId == workspaceId && !b.IsDeleted && !b.Workspace.IsDeleted)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Board>> GetWorkspaceBoardsForUserAsync(Guid workspaceId, Guid userId)
    {
        return await _context.Boards
            .Include(b => b.Creator)
            .Include(b => b.BoardMembers)
                .ThenInclude(bm => bm.User)
            .Include(b => b.Lists
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Position))
                .ThenInclude(l => l.Cards
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Position))
            .Where(b => b.WorkspaceId == workspaceId && !b.IsDeleted && !b.Workspace.IsDeleted &&
                (!b.IsPrivate || b.BoardMembers.Any(bm => bm.UserId == userId)))
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Board> CreateAsync(Board board)
    {
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();
        return board;
    }

    public async Task<Board> UpdateAsync(Board board)
    {
        _context.Boards.Update(board);
        await _context.SaveChangesAsync();
        return board;
    }

    public async Task DeleteAsync(Guid id)
    {
        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (board != null)
        {
            board.IsDeleted = true;
            _context.Boards.Update(board);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Boards.AnyAsync(b => b.Id == id);
    }

    public async Task<int> GetCardCountByBoardIdAsync(Guid boardId)
    {
        return await _context.Cards
            .Where(c => c.List.BoardId == boardId
                        && !c.IsDeleted
                        && !c.List.IsDeleted
                        && !c.List.Board.IsDeleted
                        && !c.List.Board.Workspace.IsDeleted)
            .CountAsync();
    }

    public async Task<BoardMember> AssignUserToBoardAsync(BoardMember boardMember)
    {
        _context.BoardMembers.Add(boardMember);
        await _context.SaveChangesAsync();
        
        return await _context.BoardMembers
            .Include(bm => bm.User)
            .Include(bm => bm.AssignedByUser)
            .FirstAsync(bm => bm.Id == boardMember.Id);
    }

    public async Task<bool> UnassignUserFromBoardAsync(Guid boardId, Guid userId)
    {
        var boardMember = await _context.BoardMembers
            .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
        
        if (boardMember == null) return false;
        
        _context.BoardMembers.Remove(boardMember);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BoardMember>> GetBoardMembersAsync(Guid boardId)
    {
        return await _context.BoardMembers
            .Include(bm => bm.User)
            .Include(bm => bm.AssignedByUser)
            .Where(bm => bm.BoardId == boardId)
            .ToListAsync();
    }

    public async Task<bool> IsUserBoardMemberAsync(Guid boardId, Guid userId)
    {
        return await _context.BoardMembers
            .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
    }
}