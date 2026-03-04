

namespace Eirene.BLL.Services.Abstraction.Identity;

public interface IEmailSender
{
    Task SendEmailAsync(string? to, string subject, string body);
}
