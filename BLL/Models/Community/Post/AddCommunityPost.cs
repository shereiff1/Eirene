namespace BLL.Models.Community.Post;

public class AddCommunityPost
{
    public Guid CommunityGroupId { get; set; }
    public string Content { get; set; } = string.Empty;
}