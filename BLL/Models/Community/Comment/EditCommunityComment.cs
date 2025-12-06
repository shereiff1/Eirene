namespace BLL.Models.Community.Comment;

public class EditCommunityComment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}