using Domain.Enums;

namespace Domain.Helpers;

public static class ActivityTypeHelper
{
    public static ActivityType Parse(string eventName)
    {
        // exp: "BoardCreated" -> "BoardCreated" enum
        if (Enum.TryParse<ActivityType>(eventName, out var activityType))
        {
            return activityType;
        }

        throw new ArgumentOutOfRangeException(nameof(eventName), eventName, "Unknown event name");
    }
}
