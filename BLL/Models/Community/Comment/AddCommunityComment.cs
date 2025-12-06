namespace BLL.Models.Community.Comment;

public class AddCommunityComment
{
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}