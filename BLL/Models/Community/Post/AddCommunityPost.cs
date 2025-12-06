namespace BLL.Models.Community.Post;

public class AddCommunityPost
{
    public int CommunityGroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}