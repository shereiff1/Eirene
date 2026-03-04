namespace Eirene.BLL.Models.Core.Admin;

public class AdminModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; }
}