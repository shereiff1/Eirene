
namespace Eirene.BLL.Models.Community.Group;

public class EditCommunityGroup
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
