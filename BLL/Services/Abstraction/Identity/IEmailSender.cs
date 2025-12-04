using System.Threading.Tasks;

namespace BLL.Services.Abstraction.Identity
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
