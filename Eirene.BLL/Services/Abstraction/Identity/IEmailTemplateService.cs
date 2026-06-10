namespace Eirene.BLL.Services.Abstraction.Identity;

public interface IEmailTemplateService
{
    string GetEmailTemplate(string templateName, Dictionary<string, string> placeholders);
}
