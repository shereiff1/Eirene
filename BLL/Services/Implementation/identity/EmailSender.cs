using BLL.Models.Identity;
using BLL.Services.Abstraction.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public EmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient
        {
            Host = _settings.Host,
            Port = _settings.Port,
            EnableSsl = _settings.EnableSSL,
            Credentials = new NetworkCredential(
                _settings.UserName,
                _settings.Password)
        };

        var mail = new MailMessage(_settings.UserName, to, subject, body)
        {
            IsBodyHtml = true
        };

        await Task.Delay(1500);
        await client.SendMailAsync(mail);
    }
}