namespace BLL.Models.Community.Post;

public class EditCommunityPost
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}