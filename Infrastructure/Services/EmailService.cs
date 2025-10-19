using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailService(IConfiguration _configuration) : IEmailService
{
    public async Task SendInvitationEmailAsync(string recipientEmail, string recipientName, 
                                             string inviterName, string workspaceName, 
                                             string invitationToken, DateTime expiresAt)
    {
        var subject = $"{inviterName} sizi {workspaceName} çalışma alanına davet ediyor";
        
        var body = BuildInvitationEmailBody(recipientName, inviterName, workspaceName, 
                                          invitationToken, expiresAt);

        await SendEmailAsync(recipientEmail, subject, body, isHtml: true);
    }

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var subject = "Hoşgeldiniz!";
        var body = BuildWelcomeEmailBody(name);
        
        await SendEmailAsync(email, subject, body, isHtml: true);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var subject = "Şifre Sıfırlama";
        var body = BuildPasswordResetEmailBody(resetToken);
        
        await SendEmailAsync(email, subject, body, isHtml: true);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            var fromEmail = smtpSettings["FromEmail"];
            var fromName = smtpSettings["FromName"];
            var smtpHost = smtpSettings["SmtpHost"];
            var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
            var smtpUsername = smtpSettings["SmtpUsername"];
            var smtpPassword = smtpSettings["SmtpPassword"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            using var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = enableSsl;

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail!, fromName);
            mailMessage.To.Add(toEmail);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = isHtml;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.SubjectEncoding = Encoding.UTF8;

            await smtpClient.SendMailAsync(mailMessage);         
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Email gönderilemedi: {ex.Message}");
        }
    }

    private string BuildInvitationEmailBody(string? recipientName, string inviterName, 
                                           string workspaceName, string invitationToken, 
                                           DateTime expiresAt)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var inviteUrl = $"{frontendUrl}/accept-invitation?token={invitationToken}";

        var displayName = recipientName ?? "Değerli Kullanıcı";
        var expiryDate = expiresAt.ToString("dd/MM/yyyy HH:mm");
        
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='tr'>");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset='UTF-8'>");
        sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("    <title>Çalışma Alanı Daveti</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
        sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }");
        sb.AppendLine("        .content { padding: 30px; }");
        sb.AppendLine("        .button { display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 12px 30px; border-radius: 25px; font-weight: bold; margin: 20px 0; }");
        sb.AppendLine("        .workspace-info { background-color: #f8f9fa; border-left: 4px solid #667eea; padding: 15px; margin: 20px 0; border-radius: 0 5px 5px 0; }");
        sb.AppendLine("        .footer { background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; }");
        sb.AppendLine("        .expires { color: #e74c3c; font-weight: bold; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class='container'>");
        sb.AppendLine("        <div class='header'>");
        sb.AppendLine("            <h1>🚀 Çalışma Alanı Daveti</h1>");
        sb.AppendLine("            <p>Yeni bir maceraya katılmaya hazır mısınız?</p>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class='content'>");
        sb.AppendLine($"            <p>Merhaba <strong>{displayName}</strong>,</p>");
        sb.AppendLine($"            <p><strong>{inviterName}</strong> sizi aşağıdaki çalışma alanına katılmaya davet ediyor:</p>");
        sb.AppendLine("            <div class='workspace-info'>");
        sb.AppendLine($"                <h3>📋 {workspaceName}</h3>");
        sb.AppendLine("                <p>Bu çalışma alanında takım arkadaşlarınızla birlikte projelerinizi yönetebilir, görevleri takip edebilir ve verimli bir şekilde işbirliği yapabilirsiniz.</p>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <p>Daveti kabul etmek için aşağıdaki butona tıklayın:</p>");
        sb.AppendLine("            <div style='text-align: center; margin: 30px 0;'>");
        sb.AppendLine($"                <a href='{inviteUrl}' class='button'>🎉 Daveti Kabul Et</a>");
        sb.AppendLine("            </div>");
        sb.AppendLine($"            <p><strong>Önemli:</strong> Bu davet linki <span class='expires'>{expiryDate}</span> tarihine kadar geçerlidir.</p>");
        sb.AppendLine("            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>");
        sb.AppendLine("            <p><small>Eğer bu daveti siz talep etmediyseniz, bu e-postayı görmezden gelebilirsiniz.</small></p>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class='footer'>");
        sb.AppendLine("            <p>© 2024 Workspace Management System</p>");
        sb.AppendLine("            <p>Bu e-posta otomatik olarak gönderilmiştir, lütfen yanıtlamayın.</p>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string BuildWelcomeEmailBody(string name)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='tr'>");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset='UTF-8'>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
        sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .header { text-align: center; color: #667eea; margin-bottom: 30px; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class='container'>");
        sb.AppendLine("        <div class='header'>");
        sb.AppendLine("            <h1>🎉 Hoşgeldiniz!</h1>");
        sb.AppendLine("        </div>");
        sb.AppendLine($"        <p>Merhaba <strong>{name}</strong>,</p>");
        sb.AppendLine("        <p>Sisteme başarıyla kayıt oldunuz. Artık çalışma alanlarınızı oluşturabilir ve takım arkadaşlarınızla işbirliği yapabilirsiniz.</p>");
        sb.AppendLine("        <p>İyi çalışmalar!</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string BuildPasswordResetEmailBody(string resetToken)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}";
        
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='tr'>");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset='UTF-8'>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
        sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .button { display: inline-block; background-color: #667eea; color: white; text-decoration: none; padding: 12px 30px; border-radius: 5px; margin: 20px 0; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class='container'>");
        sb.AppendLine("        <h1>🔒 Şifre Sıfırlama</h1>");
        sb.AppendLine("        <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>");
        sb.AppendLine($"        <a href='{resetUrl}' class='button'>Şifremi Sıfırla</a>");
        sb.AppendLine("        <p><strong>Bu link 1 saat geçerlidir.</strong></p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
