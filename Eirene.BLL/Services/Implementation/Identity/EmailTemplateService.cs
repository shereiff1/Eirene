using Eirene.BLL.Services.Abstraction.Identity;

namespace Eirene.BLL.Services.Implementation.Identity;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _templatePath;

    public EmailTemplateService()
    {
        // For production/build output
        _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "EmailTemplates");
        
        // For development environment
        if (!Directory.Exists(_templatePath))
        {
            var projectRoot = Directory.GetCurrentDirectory();
            // If running from API directory or solution root, find the BLL path
            if (projectRoot.EndsWith("Eirene.API"))
            {
                _templatePath = Path.Combine(Directory.GetParent(projectRoot)!.FullName, "Eirene.BLL", "Resources", "EmailTemplates");
            }
            else
            {
                _templatePath = Path.Combine(projectRoot, "Eirene.BLL", "Resources", "EmailTemplates");
            }
        }
    }

    public string GetEmailTemplate(string templateName, Dictionary<string, string> placeholders)
    {
        var filePath = Path.Combine(_templatePath, $"{templateName}.html");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Email template {templateName} not found at {filePath}");
        }

        var template = File.ReadAllText(filePath);

        foreach (var placeholder in placeholders)
        {
            template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        return template;
    }
}
