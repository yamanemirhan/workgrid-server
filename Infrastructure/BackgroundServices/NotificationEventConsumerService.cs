using Domain.Entities;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.Json;

namespace Infrastructure.BackgroundServices;

public class NotificationEventConsumerService(IRabbitMqConsumer _rabbitMqConsumer,
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
            var method = GetType().GetMethod(nameof(RegisterNotificationConsumer), BindingFlags.NonPublic | BindingFlags.Instance);
            var generic = method!.MakeGenericMethod(eventType);
            generic.Invoke(this, null);
        }

        return Task.CompletedTask;
    }

    private void RegisterNotificationConsumer<TEvent>()
    {
        _rabbitMqConsumer.StartConsuming<TEvent>(async evt =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            try
            {
                Console.WriteLine($"[NotificationConsumer] Processing event: {typeof(TEvent).Name}");
                await CreateNotificationForEvent(evt, notificationService, notificationRepository);
                Console.WriteLine($"[NotificationConsumer] Successfully created notification for {typeof(TEvent).Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationConsumer] Error creating notification for {typeof(TEvent).Name}: {ex.Message}");
            }
        }, "notification"); // Use 'notification' prefix for queue names
    }

    private async Task CreateNotificationForEvent<TEvent>(TEvent evt, INotificationService notificationService, INotificationRepository notificationRepository)
    {
        string typeName = typeof(TEvent).Name;

        var userId = (Guid)evt!.GetType().GetProperty("UserId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var description = (string?)evt.GetType().GetProperty("Description")?.GetValue(evt) ?? "";

        var workspaceMembers = await notificationRepository.GetWorkspaceMembersAsync(workspaceId);

        switch (typeName)
        {
            case "MemberInvitedEvent":
                await HandleMemberInvitedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "MemberJoinedEvent":
                await HandleMemberJoinedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "MemberRemovedEvent":
                await HandleMemberRemovedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "MemberLeftEvent":
                await HandleMemberLeftEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "MemberRoleChangedEvent":
                await HandleMemberRoleChangedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "WorkspaceCreatedEvent":
                await HandleWorkspaceCreatedEvent(evt, notificationService, userId);
                break;

            case "WorkspaceUpdatedEvent":
                await HandleWorkspaceUpdatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "BoardCreatedEvent":
                await HandleBoardCreatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "BoardUpdatedEvent":
                await HandleBoardUpdatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "BoardDeletedEvent":
                await HandleBoardDeletedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "BoardMemberAssignedEvent":
                await HandleBoardMemberAssignedEvent(evt, notificationService, userId);
                break;

            case "BoardMemberUnassignedEvent":
                await HandleBoardMemberUnassignedEvent(evt, notificationService, userId);
                break;

            case "ListCreatedEvent":
                await HandleListCreatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "ListUpdatedEvent":
                await HandleListUpdatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "ListDeletedEvent":
                await HandleListDeletedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "CardCreatedEvent":
                await HandleCardCreatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "CardUpdatedEvent":
                await HandleCardUpdatedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "CardDeletedEvent":
                await HandleCardDeletedEvent(evt, notificationService, workspaceMembers, userId);
                break;

            case "CardMemberAssignedEvent":
                await HandleCardMemberAssignedEvent(evt, notificationService, userId);
                break;

            case "CardMemberUnassignedEvent":
                await HandleCardMemberUnassignedEvent(evt, notificationService, userId);
                break;

            case "CardStatusChangedEvent":
                await HandleCardStatusChangedEvent(evt, notificationService, workspaceMembers, userId);
                break;
        }
    }

    // Member Event Handlers
    private async Task HandleMemberInvitedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var inviterName = (string)evt!.GetType().GetProperty("InviterName")!.GetValue(evt)!;
        var workspaceName = (string)evt.GetType().GetProperty("WorkspaceName")!.GetValue(evt)!;
        var invitedEmail = (string)evt.GetType().GetProperty("InvitedEmail")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.MemberInvited,
                "New Member Invited",
                $"{inviterName} invited {invitedEmail} to {workspaceName}",
                JsonSerializer.Serialize(new { InvitedEmail = invitedEmail }),
                workspaceId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleMemberJoinedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var joinedUserName = (string)evt!.GetType().GetProperty("JoinedUserName")!.GetValue(evt)!;
        var workspaceName = (string)evt.GetType().GetProperty("WorkspaceName")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var joinedUserId = (Guid)evt.GetType().GetProperty("JoinedUserId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != joinedUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.MemberJoined,
                "New Member Joined",
                $"{joinedUserName} joined {workspaceName}",
                JsonSerializer.Serialize(new { JoinedUserName = joinedUserName }),
                workspaceId,
                relatedUserId: joinedUserId
            );
        }
    }

    private async Task HandleMemberRemovedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var removedUserName = (string)evt!.GetType().GetProperty("RemovedUserName")!.GetValue(evt)!;
        var removedByName = (string)evt.GetType().GetProperty("RemovedByName")!.GetValue(evt)!;
        var workspaceName = (string)evt.GetType().GetProperty("WorkspaceName")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var removedUserId = (Guid)evt.GetType().GetProperty("RemovedUserId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId && m.UserId != removedUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.MemberRemoved,
                "Member Removed",
                $"{removedByName} removed {removedUserName} from {workspaceName}",
                JsonSerializer.Serialize(new { RemovedUserName = removedUserName, RemovedByName = removedByName }),
                workspaceId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleMemberLeftEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var leftUserName = (string)evt!.GetType().GetProperty("LeftUserName")!.GetValue(evt)!;
        var workspaceName = (string)evt.GetType().GetProperty("WorkspaceName")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var leftUserId = (Guid)evt.GetType().GetProperty("LeftUserId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != leftUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.MemberLeft,
                "Member Left",
                $"{leftUserName} left {workspaceName}",
                JsonSerializer.Serialize(new { LeftUserName = leftUserName }),
                workspaceId,
                relatedUserId: leftUserId
            );
        }
    }

    private async Task HandleMemberRoleChangedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var targetUserName = (string)evt!.GetType().GetProperty("TargetUserName")!.GetValue(evt)!;
        var changedByName = (string)evt.GetType().GetProperty("ChangedByName")!.GetValue(evt)!;
        var oldRole = (WorkspaceRole)evt.GetType().GetProperty("OldRole")!.GetValue(evt)!;
        var newRole = (WorkspaceRole)evt.GetType().GetProperty("NewRole")!.GetValue(evt)!;
        var workspaceName = (string)evt.GetType().GetProperty("WorkspaceName")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var targetUserId = (Guid)evt.GetType().GetProperty("TargetUserId")!.GetValue(evt)!;

        await notificationService.CreateNotificationAsync(
            targetUserId,
            NotificationType.MemberRoleChanged,
            "Your Role Changed",
            $"{changedByName} changed your role from {oldRole} to {newRole} in {workspaceName}",
            JsonSerializer.Serialize(new { OldRole = oldRole.ToString(), NewRole = newRole.ToString(), ChangedByName = changedByName }),
            workspaceId,
            relatedUserId: triggerUserId
        );

        foreach (var member in members.Where(m => m.UserId != triggerUserId && m.UserId != targetUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.MemberRoleChanged,
                "Member Role Changed",
                $"{changedByName} changed {targetUserName}'s role from {oldRole} to {newRole}",
                JsonSerializer.Serialize(new { TargetUserName = targetUserName, OldRole = oldRole.ToString(), NewRole = newRole.ToString() }),
                workspaceId,
                relatedUserId: triggerUserId
            );
        }
    }

    // Board Member Assignment Event Handlers
    private async Task HandleBoardMemberAssignedEvent<TEvent>(TEvent evt, INotificationService notificationService, Guid triggerUserId)
    {
        var boardTitle = (string?)evt!.GetType().GetProperty("BoardTitle")?.GetValue(evt) ?? "Board";
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var assignedUserId = (Guid)evt.GetType().GetProperty("AssignedUserId")!.GetValue(evt)!;

        await notificationService.CreateNotificationAsync(
            assignedUserId,
            NotificationType.BoardMemberAssigned,
            "You were assigned to a board",
            $"You have been assigned to board '{boardTitle}'",
            JsonSerializer.Serialize(new { BoardTitle = boardTitle, AssignedByUserId = triggerUserId }),
            workspaceId,
            boardId,
            relatedUserId: triggerUserId
        );
    }

    private async Task HandleBoardMemberUnassignedEvent<TEvent>(TEvent evt, INotificationService notificationService, Guid triggerUserId)
    {
        var boardTitle = (string?)evt!.GetType().GetProperty("BoardTitle")?.GetValue(evt) ?? "Board";
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var unassignedUserId = (Guid)evt.GetType().GetProperty("UnassignedUserId")!.GetValue(evt)!;

        await notificationService.CreateNotificationAsync(
            unassignedUserId,
            NotificationType.BoardMemberUnassigned,
            "You were unassigned from a board",
            $"You have been unassigned from board '{boardTitle}'",
            JsonSerializer.Serialize(new { BoardTitle = boardTitle, UnassignedByUserId = triggerUserId }),
            workspaceId,
            boardId,
            relatedUserId: triggerUserId
        );
    }

    // Card Member Assignment Event Handlers
    private async Task HandleCardMemberAssignedEvent<TEvent>(TEvent evt, INotificationService notificationService, Guid triggerUserId)
    {
        var cardTitle = (string?)evt!.GetType().GetProperty("CardTitle")?.GetValue(evt) ?? "Card";
        var cardId = (Guid)evt.GetType().GetProperty("CardId")!.GetValue(evt)!;
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var assignedUserId = (Guid)evt.GetType().GetProperty("AssignedUserId")!.GetValue(evt)!;

        await notificationService.CreateNotificationAsync(
            assignedUserId,
            NotificationType.CardMemberAssigned,
            "You were assigned to a card",
            $"You have been assigned to card '{cardTitle}'",
            JsonSerializer.Serialize(new { CardTitle = cardTitle, AssignedByUserId = triggerUserId }),
            workspaceId,
            boardId,
            listId,
            cardId,
            relatedUserId: triggerUserId
        );
    }

    private async Task HandleCardMemberUnassignedEvent<TEvent>(TEvent evt, INotificationService notificationService, Guid triggerUserId)
    {
        var cardTitle = (string?)evt!.GetType().GetProperty("CardTitle")?.GetValue(evt) ?? "Card";
        var cardId = (Guid)evt.GetType().GetProperty("CardId")!.GetValue(evt)!;
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var unassignedUserId = (Guid)evt.GetType().GetProperty("UnassignedUserId")!.GetValue(evt)!;

        await notificationService.CreateNotificationAsync(
            unassignedUserId,
            NotificationType.CardMemberUnassigned,
            "You were unassigned from a card",
            $"You have been unassigned from card '{cardTitle}'",
            JsonSerializer.Serialize(new { CardTitle = cardTitle, UnassignedByUserId = triggerUserId }),
            workspaceId,
            boardId,
            listId,
            cardId,
            relatedUserId: triggerUserId
        );
    }

    // Workspace Event Handlers
    private async Task HandleWorkspaceCreatedEvent<TEvent>(TEvent evt, INotificationService notificationService, Guid triggerUserId)
    {
        var workspaceName = (string)evt!.GetType().GetProperty("WorkspaceName")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        await notificationService.CreateNotificationAsync(
            triggerUserId,
            NotificationType.WorkspaceCreated,
            "Workspace Created",
            $"You successfully created workspace '{workspaceName}'",
            JsonSerializer.Serialize(new { WorkspaceName = workspaceName }),
            workspaceId,
            relatedUserId: triggerUserId
        );
    }

    private async Task HandleWorkspaceUpdatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var workspaceName = (string?)evt!.GetType().GetProperty("WorkspaceName")?.GetValue(evt) ?? "Workspace";
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.WorkspaceUpdated,
                "Workspace Updated",
                $"Workspace '{workspaceName}' has been updated",
                JsonSerializer.Serialize(new { WorkspaceName = workspaceName }),
                workspaceId,
                relatedUserId: triggerUserId
            );
        }
    }

    // Board Event Handlers
    private async Task HandleBoardCreatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var boardTitle = (string?)evt!.GetType().GetProperty("BoardTitle")?.GetValue(evt) ?? "New Board";
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.BoardCreated,
                "New Board Created",
                $"A new board '{boardTitle}' has been created",
                JsonSerializer.Serialize(new { BoardTitle = boardTitle }),
                workspaceId,
                boardId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleBoardUpdatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var boardTitle = (string?)evt!.GetType().GetProperty("BoardTitle")?.GetValue(evt) ?? "Board";
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.BoardUpdated,
                "Board Updated",
                $"Board '{boardTitle}' has been updated",
                JsonSerializer.Serialize(new { BoardTitle = boardTitle }),
                workspaceId,
                boardId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleBoardDeletedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var boardTitle = (string?)evt!.GetType().GetProperty("BoardTitle")?.GetValue(evt) ?? "Board";
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.BoardDeleted,
                "Board Deleted",
                $"Board '{boardTitle}' has been deleted",
                JsonSerializer.Serialize(new { BoardTitle = boardTitle }),
                workspaceId,
                relatedUserId: triggerUserId
            );
        }
    }

    // List Event Handlers
    private async Task HandleListCreatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var listTitle = (string?)evt!.GetType().GetProperty("ListTitle")?.GetValue(evt) ?? "New List";
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.ListCreated,
                "New List Created",
                $"A new list '{listTitle}' has been created",
                JsonSerializer.Serialize(new { ListTitle = listTitle }),
                workspaceId,
                boardId,
                listId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleListUpdatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var listTitle = (string?)evt!.GetType().GetProperty("ListTitle")?.GetValue(evt) ?? "List";
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.ListUpdated,
                "List Updated",
                $"List '{listTitle}' has been updated",
                JsonSerializer.Serialize(new { ListTitle = listTitle }),
                workspaceId,
                boardId,
                listId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleListDeletedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var listTitle = (string?)evt!.GetType().GetProperty("ListTitle")?.GetValue(evt) ?? "List";
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.ListDeleted,
                "List Deleted",
                $"List '{listTitle}' has been deleted",
                JsonSerializer.Serialize(new { ListTitle = listTitle }),
                workspaceId,
                boardId,
                relatedUserId: triggerUserId
            );
        }
    }

    // Card Event Handlers
    private async Task HandleCardCreatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var cardTitle = (string?)evt!.GetType().GetProperty("CardTitle")?.GetValue(evt) ?? "New Card";
        var cardId = (Guid)evt.GetType().GetProperty("CardId")!.GetValue(evt)!;
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.CardCreated,
                "New Card Created",
                $"A new card '{cardTitle}' has been created",
                JsonSerializer.Serialize(new { CardTitle = cardTitle }),
                workspaceId,
                boardId,
                listId,
                cardId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleCardUpdatedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var cardTitle = (string?)evt!.GetType().GetProperty("CardTitle")?.GetValue(evt) ?? "Card";
        var cardId = (Guid)evt.GetType().GetProperty("CardId")!.GetValue(evt)!;
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.CardUpdated,
                "Card Updated",
                $"Card '{cardTitle}' has been updated",
                JsonSerializer.Serialize(new { CardTitle = cardTitle }),
                workspaceId,
                boardId,
                listId,
                cardId,
                relatedUserId: triggerUserId
            );
        }
    }

    private async Task HandleCardDeletedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var cardTitle = (string?)evt!.GetType().GetProperty("CardTitle")?.GetValue(evt) ?? "Card";
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.CardDeleted,
                "Card Deleted",
                $"Card '{cardTitle}' has been deleted",
                JsonSerializer.Serialize(new { CardTitle = cardTitle }),
                workspaceId,
                boardId,
                listId,
                relatedUserId: triggerUserId
            );
        }
    }

    // Card Status Changed Event Handler
    private async Task HandleCardStatusChangedEvent<TEvent>(TEvent evt, INotificationService notificationService, IEnumerable<WorkspaceMember> members, Guid triggerUserId)
    {
        var cardTitle = (string?)evt!.GetType().GetProperty("CardTitle")?.GetValue(evt) ?? "Card";
        var cardId = (Guid)evt.GetType().GetProperty("CardId")!.GetValue(evt)!;
        var listId = (Guid)evt.GetType().GetProperty("ListId")!.GetValue(evt)!;
        var boardId = (Guid)evt.GetType().GetProperty("BoardId")!.GetValue(evt)!;
        var workspaceId = (Guid)evt.GetType().GetProperty("WorkspaceId")!.GetValue(evt)!;
        var oldStatusName = (string?)evt.GetType().GetProperty("OldStatusName")?.GetValue(evt) ?? "No Status";
        var newStatusName = (string?)evt.GetType().GetProperty("NewStatusName")?.GetValue(evt) ?? "Unknown Status";

        foreach (var member in members.Where(m => m.UserId != triggerUserId))
        {
            await notificationService.CreateNotificationAsync(
                member.UserId,
                NotificationType.CardUpdated,
                "Card Status Changed",
                $"Card '{cardTitle}' status changed from '{oldStatusName}' to '{newStatusName}'",
                JsonSerializer.Serialize(new { CardTitle = cardTitle, OldStatus = oldStatusName, NewStatus = newStatusName }),
                workspaceId,
                boardId,
                listId,
                cardId,
                relatedUserId: triggerUserId
            );
        }
    }
}