using DAL.Entities.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Community;

public class CommunityComment
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int PostId { get; set; }
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    public DateTime PostedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; }
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public int? ParentCommentId { get; set; }
    public int LikesCount { get; set; } = 0;
    public int RepliesCount { get; set; } = 0;
    [ForeignKey(nameof(PostId))]
    public CommunityPost? Post { get; set; }
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
    [ForeignKey(nameof(ParentCommentId))]
    public CommunityComment? ParentComment { get; set; }
    public ICollection<CommunityComment> Replies { get; set; } = new List<CommunityComment>();

}

