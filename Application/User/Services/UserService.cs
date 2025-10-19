using Infrastructure.Repositories;
using Shared.DTOs;
using Domain.Enums;

namespace Application.User.Services;

public interface IUserService
{
    Task<UserDetailDto> GetCurrentUserWithDetailsAsync(Guid userId);
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<UserDto> GetUserByEmailAsync(string email);
}

public class UserService(IUserRepository _userRepository) : IUserService
{
    public async Task<UserDetailDto> GetCurrentUserWithDetailsAsync(Guid userId)
    {
        var user = await _userRepository.GetUserWithAllDetailsAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        // Kullan?c?n?n eri?ebildi?i tüm unique board'lar? hesapla
        var allBoardIds = new HashSet<Guid>();

        foreach (var workspace in user.OwnedWorkspaces)
        {
            foreach (var board in workspace.Boards)
            {
                allBoardIds.Add(board.Id);
            }
        }

        foreach (var member in user.WorkspaceMembers)
        {
            foreach (var board in member.Workspace.Boards)
            {
                allBoardIds.Add(board.Id);
            }
        }

        // Kullan?c?n?n eri?ebildi?i tüm unique card'lar? hesapla
        var allCardIds = new HashSet<Guid>();

        foreach (var workspace in user.OwnedWorkspaces)
        {
            foreach (var board in workspace.Boards)
            {
                foreach (var list in board.Lists)
                {
                    foreach (var card in list.Cards)
                    {
                        allCardIds.Add(card.Id);
                    }
                }
            }
        }

        foreach (var member in user.WorkspaceMembers)
        {
            foreach (var board in member.Workspace.Boards)
            {
                foreach (var list in board.Lists)
                {
                    foreach (var card in list.Cards)
                    {
                        allCardIds.Add(card.Id);
                    }
                }
            }
        }

        return new UserDetailDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,

            // Workspace memberships
            WorkspaceMemberships = user.WorkspaceMembers.Select(wm => new WorkspaceMembershipDto
            {
                WorkspaceId = wm.WorkspaceId,
                WorkspaceName = wm.Workspace.Name,
                WorkspaceLogo = wm.Workspace.Logo,
                Role = (WorkspaceRole)wm.Role,
                JoinedAt = wm.JoinedAt
            }).ToList(),

            // Owned workspaces
            OwnedWorkspaces = user.OwnedWorkspaces.Select(w => new WorkspaceDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                Logo = w.Logo,
                OwnerId = w.OwnerId,
                OwnerName = w.Owner.Name,
                CreatedAt = w.CreatedAt,
                MemberCount = w.Members.Count,
                BoardCount = w.Boards.Count
            }).ToList(),

            // Total statistics
            TotalBoardsCount = allBoardIds.Count,
            TotalCardsCount = allCardIds.Count
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt
        };
    }
}