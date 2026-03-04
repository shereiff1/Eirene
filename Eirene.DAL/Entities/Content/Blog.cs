using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eirene.DAL.Entities.Core;

namespace Eirene.DAL.Entities.Content;

public class Blog
{
    [Key] public Guid Id { get; set; }
    [Required] public string DoctorId { get; set; } = string.Empty;
    [ForeignKey(nameof(DoctorId))] public ApplicationUser? Doctor { get; set; }
    [Required] public string BlogContent { get; set; } = string.Empty;
    [Required]public string Title { get; set; } = string.Empty;
    [Required] public string Topic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
