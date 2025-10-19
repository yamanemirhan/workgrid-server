using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ListRepository(AppDbContext _context) : IListRepository
{
    public async Task<List?> GetByIdAsync(Guid id)
    {
        return await _context.Lists.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
    }

    public async Task<List?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Lists
            .Include(l => l.Board)
            .Include(l => l.Cards
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Position))
                .ThenInclude(c => c.Creator)
            .Include(l => l.Cards)
                .ThenInclude(c => c.CardMembers)
                    .ThenInclude(cm => cm.User)
            .Include(l => l.Cards)
                .ThenInclude(c => c.CardMembers)
                    .ThenInclude(cm => cm.AssignedByUser)
            .Include(l => l.Cards)
                .ThenInclude(c => c.CardFollowers)
                    .ThenInclude(cf => cf.User)
            .FirstOrDefaultAsync(l => l.Id == id
                                      && !l.IsDeleted
                                      && !l.Board.IsDeleted
                                      && !l.Board.Workspace.IsDeleted);
    }

    public async Task<IEnumerable<List>> GetBoardListsAsync(Guid boardId)
    {
        return await _context.Lists
            .Include(l => l.Board)
            .Include(l => l.Cards.Where(c => !c.IsDeleted).OrderBy(c => c.Position))
                .ThenInclude(c => c.Creator)
            .Include(l => l.Cards)
                .ThenInclude(c => c.CardMembers)
                    .ThenInclude(cm => cm.User)
            .Include(l => l.Cards)
                .ThenInclude(c => c.CardMembers)
                    .ThenInclude(cm => cm.AssignedByUser)
            .Include(l => l.Cards)
                .ThenInclude(c => c.CardFollowers)
                    .ThenInclude(cf => cf.User)
            .Where(l => l.BoardId == boardId
                        && !l.IsDeleted
                        && !l.Board.IsDeleted
                        && !l.Board.Workspace.IsDeleted)
            .OrderBy(l => l.Position)
            .ToListAsync();
    }

    public async Task<List> CreateAsync(List list)
    {
        _context.Lists.Add(list);
        await _context.SaveChangesAsync();
        return list;
    }

    public async Task<List> UpdateAsync(List list)
    {
        _context.Lists.Update(list);
        await _context.SaveChangesAsync();
        return list;
    }

    public async Task DeleteAsync(Guid id)
    {
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == id);
        if (list != null)
        {
            list.IsDeleted = true;
            _context.Lists.Update(list);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Lists
            .AnyAsync(l => l.Id == id
                           && !l.IsDeleted
                           && !l.Board.IsDeleted
                           && !l.Board.Workspace.IsDeleted);
    }

    public async Task<int> GetNextPositionAsync(Guid boardId)
    {
        var maxPosition = await _context.Lists
            .Where(l => l.BoardId == boardId
                        && !l.IsDeleted
                        && !l.Board.IsDeleted
                        && !l.Board.Workspace.IsDeleted)
            .MaxAsync(l => (int?)l.Position);

        return (maxPosition ?? 0) + 1;
    }

    public async Task<int> GetCardCountByListIdAsync(Guid listId)
    {
        return await _context.Cards
            .Where(c => c.ListId == listId
                        && !c.IsDeleted
                        && !c.List.IsDeleted
                        && !c.List.Board.IsDeleted
                        && !c.List.Board.Workspace.IsDeleted)
            .CountAsync();
    }
}