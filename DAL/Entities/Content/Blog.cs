using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Entities.Core;

namespace DAL.Entities.Content;

public class Blog
{
    [Key] public Guid Id { get; set; }
    [Required] public string DoctorId { get; set; } = string.Empty;
    [ForeignKey(nameof(DoctorId))] public ApplicationUser Doctor { get; set; } = new ApplicationUser();
    [Required] public string BlogContent { get; set; } = string.Empty;
    [Required]public string Title { get; set; } = string.Empty;
    [Required] public string Topic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
