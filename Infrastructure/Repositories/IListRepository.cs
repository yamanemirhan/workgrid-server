using Domain.Entities;

namespace Infrastructure.Repositories;

public interface IListRepository
{
    Task<List?> GetByIdAsync(Guid id);
    Task<List?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<List>> GetBoardListsAsync(Guid boardId);
    Task<List> CreateAsync(List list);
    Task<List> UpdateAsync(List list);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetNextPositionAsync(Guid boardId);
    Task<int> GetCardCountByListIdAsync(Guid listId);
}