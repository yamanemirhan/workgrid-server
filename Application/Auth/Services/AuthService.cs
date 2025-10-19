using Microsoft.Extensions.Configuration;
using Infrastructure.Repositories;
using Shared.DTOs;
using Shared.Utils;

namespace Application.Auth.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(string name, string email, string password);
    Task<LoginResponseDto> LoginAsync(string email, string password);
    Task<bool> LogoutAsync();
}

public class AuthService(IUserRepository _userRepository, IConfiguration _configuration) : IAuthService
{
    public async Task<UserDto> RegisterAsync(string name, string email, string password)
    {
        if (await _userRepository.ExistsByEmailAsync(email))
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        var user = new Domain.Entities.User
        {
            Name = name,
            Email = email.ToLower(),
            PasswordHash = PasswordHasher.HashPassword(password)
        };

        var createdUser = await _userRepository.CreateAsync(user);

        return new UserDto
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email,
            Avatar = createdUser.Avatar,
            CreatedAt = createdUser.CreatedAt
        };
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var jwtSecret = _configuration["JwtSettings:Secret"] ?? "default-secret-key-32-characters";
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "1440");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "TrelloClone";
        var audience = _configuration["JwtSettings:Audience"] ?? "TrelloClone";

        var token = JwtHelper.GenerateToken(user.Id, user.Email, user.Name, jwtSecret, expiryMinutes, issuer, audience);

        return new LoginResponseDto
        {
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            },
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task<bool> LogoutAsync()
    {
        return await Task.FromResult(true);
    }
}
