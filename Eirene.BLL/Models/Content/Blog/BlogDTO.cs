using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.ModelVMs.Content;

public class BlogDTO
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string BlogContent { get; set; } = string.Empty;
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Topic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
