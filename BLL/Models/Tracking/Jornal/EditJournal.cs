using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Tracking;

public class EditJournal
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    public string Content { get; set; } = string.Empty;

    public float Mood { get; set; }
}