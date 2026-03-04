

using Eirene.BLL.Models.Community.Post;
using Eirene.BLL.Models.Identity;

namespace Eirene.BLL.Models.Community.Group;

public class CommunityGroupWithDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public int PostsCount { get; set; }
    public ICollection<CommunityPostDTO> Posts { get; set; } = new List<CommunityPostDTO>();
    public ICollection<UserDTO> Members { get; set; } = new List<UserDTO>();
}
