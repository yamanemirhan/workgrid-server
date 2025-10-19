using Domain.Entities;
using ActivityService.Repositories;

namespace ActivityService.Services;

public class ActivityService(IActivityRepository _activityRepository) : IActivityService
{
    public async Task<IEnumerable<Activity>> GetActivitiesByWorkspaceIdAsync(Guid workspaceId, int page = 1, int pageSize = 50)
    {
        return await _activityRepository.GetActivitiesByWorkspaceIdAsync(workspaceId, page, pageSize);
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByBoardIdAsync(Guid boardId, int page = 1, int pageSize = 50)
    {
        return await _activityRepository.GetActivitiesByBoardIdAsync(boardId, page, pageSize);
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByListIdAsync(Guid listId, int page = 1, int pageSize = 50)
    {
        return await _activityRepository.GetActivitiesByListIdAsync(listId, page, pageSize);
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByCardIdAsync(Guid cardId, int page = 1, int pageSize = 50)
    {
        return await _activityRepository.GetActivitiesByCardIdAsync(cardId, page, pageSize);
    }      
}