namespace BLL.Models.Community.Comment;

public class CommunityCommentDTO
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PostedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public bool IsEdited { get; set; }
    public int? ParentCommentId { get; set; }
    public int RepliesCount { get; set; }
    public List<CommunityCommentDTO>? Replies { get; set; }
}