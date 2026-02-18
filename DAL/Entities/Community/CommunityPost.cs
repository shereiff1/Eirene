using DAL.Entities.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Community;

public class CommunityPost
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid CommunityGroupId { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [Required][MaxLength(5000)] public string Content { get; set; } = string.Empty;
    public DateTime PostedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; }
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public int CommentsCount { get; set; } = 0;
    [ForeignKey(nameof(CommunityGroupId))] public CommunityGroup? CommunityGroup { get; set; }
    [ForeignKey(nameof(UserId))] public ApplicationUser? User { get; set; }
    public ICollection<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
}