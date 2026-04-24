
using Eirene.BLL.Models.Identity;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Eirene.BLL.Services.Abstraction.Identity;

namespace Eirene.BLL.Services.Implementation.identity;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public SmtpEmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string? to, string subject, string body)
    {

        try
        {
            using var tcp = new System.Net.Sockets.TcpClient();
            await tcp.ConnectAsync("smtp.gmail.com", 587);
            Console.WriteLine("SMTP reachable ");
        }
        catch (Exception ex)
        {
            Console.WriteLine("SMTP blocked: " + ex.Message);
        }
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
