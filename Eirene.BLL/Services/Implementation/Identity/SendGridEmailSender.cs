using Eirene.BLL.Models.Identity;
using Eirene.BLL.Services.Abstraction.Identity;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Eirene.BLL.Services.Implementation.Identity;

public class SendGridEmailSender : IEmailSender
{
    private readonly SendGridSettings _settings;

    public SendGridEmailSender(IOptions<SendGridSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string? to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email address cannot be null or empty.", nameof(to));

        var client = new SendGridClient(_settings.ApiKey);

        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent: null, htmlContent: body);

        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"SendGrid error ({response.StatusCode}): {responseBody}");
            throw new Exception($"SendGrid failed with status {response.StatusCode}: {responseBody}");
        }
    }
}
