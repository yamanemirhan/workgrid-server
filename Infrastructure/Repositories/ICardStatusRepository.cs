using Domain.Entities;

namespace Infrastructure.Repositories;

public interface ICardStatusRepository
{
    Task<IEnumerable<CardStatus>> GetWorkspaceStatusesAsync(Guid workspaceId);
    Task<IEnumerable<CardStatus>> GetDefaultStatusesAsync();
    Task<CardStatus?> GetByIdAsync(Guid id);
    Task<CardStatus?> GetByNameAndWorkspaceAsync(string name, Guid? workspaceId);
    Task<CardStatus> CreateAsync(CardStatus cardStatus);
    Task<CardStatus> UpdateAsync(CardStatus cardStatus);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> IsDefaultStatusAsync(Guid id);
    Task<CardStatus?> GetDefaultStatusByNameAsync(string name);
    Task<IEnumerable<CardStatus>> CreateDefaultStatusesForWorkspaceAsync(Guid workspaceId);
    Task<int> GetCardCountByStatusAsync(Guid statusId);
    Task<bool> CanDeleteStatusAsync(Guid statusId);
    Task<int> GetCardCountByStatusAndWorkspaceIdAsync(Guid statusId, Guid workspaceId);
}