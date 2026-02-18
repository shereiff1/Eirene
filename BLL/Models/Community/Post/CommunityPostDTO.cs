using BLL.Models.Community.Comment;

namespace BLL.Models.Community.Post;

public class CommunityPostDTO
{
    public Guid Id { get; set; }
    public Guid CommunityGroupId { get; set; }
    public string CommunityGroupName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Guid ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<CommunityCommentDTO>? Comments { get; set; }
    public DateTime PostedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public bool IsEdited { get; set; }
    public int CommentsCount { get; set; }

}
