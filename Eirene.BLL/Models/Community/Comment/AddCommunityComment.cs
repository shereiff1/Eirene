namespace Eirene.BLL.Models.Community.Comment;

public class AddCommunityComment
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}
