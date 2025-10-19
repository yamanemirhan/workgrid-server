namespace Infrastructure.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string recipientEmail, string recipientName, string inviterName, 
                                string workspaceName, string invitationToken, DateTime expiresAt);
    Task SendWelcomeEmailAsync(string email, string name);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}