using Eirene.DAL.Entities.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Eirene.DAL.Entities.Community;

public class CommunityGroup
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Required] public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedBy { get; set; }
    public ICollection<ApplicationUser>? Members { get; set; } = new List<ApplicationUser>();
    public ICollection<CommunityPost> Posts { get; set; } = new List<CommunityPost>();

}
