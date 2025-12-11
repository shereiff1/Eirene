using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Tracking;

public class AddJournal
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    public string Content { get; set; } = string.Empty;
    public decimal Mood { get; set; }


}