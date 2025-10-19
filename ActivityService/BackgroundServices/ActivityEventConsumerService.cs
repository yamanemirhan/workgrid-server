using ActivityService.Messaging;
using ActivityService.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using System.Reflection;

namespace ActivityService.BackgroundServices;

public class ActivityEventConsumerService(IRabbitMqConsumer _rabbitMqConsumer,
        IServiceScopeFactory _serviceScopeFactory) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventTypes = Assembly.GetAssembly(typeof(Domain.Events.WorkspaceCreatedEvent))!
            .GetTypes()
            .Where(t => t.Namespace == "Domain.Events" && t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var eventType in eventTypes)
        {
            var method = GetType().GetMethod(nameof(RegisterConsumer), BindingFlags.NonPublic | BindingFlags.Instance);
            var generic = method!.MakeGenericMethod(eventType);
            generic.Invoke(this, null);
        }

        return Task.CompletedTask;
    }

    private void RegisterConsumer<TEvent>()
    {
        _rabbitMqConsumer.StartConsuming<TEvent>(async evt =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();

            string typeName = typeof(TEvent).Name; // exp: "BoardCreatedEvent" or "MemberInvitedEvent"
            string eventName = typeName.Replace("Event", ""); // "BoardCreated" or "MemberInvited"

            try
            {
                // ActivityType enum parse
                ActivityType activityType = ActivityTypeHelper.Parse(eventName);

                string entityType = ExtractEntityType(typeName);

                var userId = (Guid)evt!.GetType().GetProperty("UserId")!.GetValue(evt)!;
                var description = (string)evt.GetType().GetProperty("Description")!.GetValue(evt)!;
                var createdAt = (DateTime)evt.GetType().GetProperty("OccurredAt")!.GetValue(evt)!;
                var metadata = (string?)evt.GetType().GetProperty("Metadata")?.GetValue(evt);

                var workspaceId = (Guid?)evt.GetType().GetProperty("WorkspaceId")?.GetValue(evt);
                var boardId = (Guid?)evt.GetType().GetProperty("BoardId")?.GetValue(evt);
                var cardId = (Guid?)evt.GetType().GetProperty("CardId")?.GetValue(evt);
                var listId = (Guid?)evt.GetType().GetProperty("ListId")?.GetValue(evt);

                var activity = new Activity
                {
                    UserId = userId,
                    WorkspaceId = workspaceId ?? Guid.Empty,
                    BoardId = boardId,
                    CardId = cardId,
                    ListId = listId,
                    Type = activityType,
                    Description = description,
                    EntityId = ResolveEntityId(evt, entityType, typeName),
                    EntityType = entityType,
                    Metadata = metadata,
                    CreatedAt = createdAt
                };

                await activityRepository.AddAsync(activity);
                Console.WriteLine($"[ActivityConsumer] Created activity for {typeName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ActivityConsumer] Error processing {typeName}: {ex.Message}");
            }
        }, "activity"); // Use 'activity' prefix for queue names
    }

    private Guid ResolveEntityId(object evt, string entityType, string typeName)
    {
        // Handle member assignment events
        if (typeName.Contains("MemberAssigned") || typeName.Contains("MemberUnassigned"))
        {
            // For assignment events, the assigned/unassigned user is the entity
            if (typeName.Contains("BoardMember"))
            {
                var assignedUserIdProp = evt.GetType().GetProperty("AssignedUserId");
                var unassignedUserIdProp = evt.GetType().GetProperty("UnassignedUserId");

                if (assignedUserIdProp != null)
                    return (Guid)assignedUserIdProp.GetValue(evt)!;
                if (unassignedUserIdProp != null)
                    return (Guid)unassignedUserIdProp.GetValue(evt)!;
            }
            else if (typeName.Contains("CardMember"))
            {
                var assignedUserIdProp = evt.GetType().GetProperty("AssignedUserId");
                var unassignedUserIdProp = evt.GetType().GetProperty("UnassignedUserId");

                if (assignedUserIdProp != null)
                    return (Guid)assignedUserIdProp.GetValue(evt)!;
                if (unassignedUserIdProp != null)
                    return (Guid)unassignedUserIdProp.GetValue(evt)!;
            }
        }

        // For member-related events, we need special handling
        if (entityType == "Member")
        {
            // For member events, try to get the appropriate user ID
            if (typeName.Contains("Invited"))
            {
                // For MemberInvitedEvent, use InvitedUserId if available, otherwise use the inviter's ID
                var invitedUserIdProp = evt.GetType().GetProperty("InvitedUserId");
                if (invitedUserIdProp != null)
                {
                    var invitedUserId = (Guid)invitedUserIdProp.GetValue(evt)!;
                    // If InvitedUserId is empty (user doesn't exist yet), use the inviter's UserId
                    return invitedUserId != Guid.Empty ? invitedUserId : (Guid)evt.GetType().GetProperty("UserId")!.GetValue(evt)!;
                }
            }
            else if (typeName.Contains("Joined"))
            {
                // For MemberJoinedEvent, use JoinedUserId
                var joinedUserIdProp = evt.GetType().GetProperty("JoinedUserId");
                if (joinedUserIdProp != null)
                {
                    return (Guid)joinedUserIdProp.GetValue(evt)!;
                }
            }
            else if (typeName.Contains("Removed"))
            {
                // For MemberRemovedEvent, use RemovedUserId
                var removedUserIdProp = evt.GetType().GetProperty("RemovedUserId");
                if (removedUserIdProp != null)
                {
                    return (Guid)removedUserIdProp.GetValue(evt)!;
                }
            }
            else if (typeName.Contains("Left"))
            {
                // For MemberLeftEvent, use LeftUserId
                var leftUserIdProp = evt.GetType().GetProperty("LeftUserId");
                if (leftUserIdProp != null)
                {
                    return (Guid)leftUserIdProp.GetValue(evt)!;
                }
            }
            else if (typeName.Contains("RoleChanged"))
            {
                // For MemberRoleChangedEvent, use TargetUserId
                var targetUserIdProp = evt.GetType().GetProperty("TargetUserId");
                if (targetUserIdProp != null)
                {
                    return (Guid)targetUserIdProp.GetValue(evt)!;
                }
            }

            // Fallback to UserId for member events
            return (Guid)evt.GetType().GetProperty("UserId")!.GetValue(evt)!;
        }

        // For non-member events, use the standard approach
        var entityIdProp = evt.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.Equals($"{entityType}Id", StringComparison.OrdinalIgnoreCase));

        return entityIdProp != null ? (Guid)entityIdProp.GetValue(evt)! : Guid.Empty;
    }

    private string ExtractEntityType(string typeName)
    {
        // Handle member assignment events specifically
        if (typeName.StartsWith("BoardMember") || typeName.StartsWith("CardMember"))
        {
            return "Member";
        }

        // Handle member events specifically
        if (typeName.StartsWith("Member"))
        {
            return "Member";
        }

        // Handle standard CRUD events
        var suffixes = new[] { "CreatedEvent", "UpdatedEvent", "DeletedEvent", "MovedEvent", "AssignedEvent", "UnassignedEvent" };
        foreach (var suffix in suffixes)
        {
            if (typeName.EndsWith(suffix))
            {
                return typeName.Replace(suffix, "");
            }
        }

        // Fallback: extract everything before "Event"
        return typeName.Replace("Event", "");
    }
}