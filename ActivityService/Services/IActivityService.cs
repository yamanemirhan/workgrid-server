using Domain.Entities;

namespace ActivityService.Services;

public interface IActivityService
{
    Task<IEnumerable<Activity>> GetActivitiesByWorkspaceIdAsync(Guid workspaceId, int page = 1, int pageSize = 50);
    Task<IEnumerable<Activity>> GetActivitiesByBoardIdAsync(Guid boardId, int page = 1, int pageSize = 50);
    Task<IEnumerable<Activity>> GetActivitiesByCardIdAsync(Guid cardId, int page = 1, int pageSize = 50);
    Task<IEnumerable<Activity>> GetActivitiesByListIdAsync(Guid listId, int page = 1, int pageSize = 50);
}