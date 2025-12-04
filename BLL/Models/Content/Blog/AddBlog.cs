using System.ComponentModel.DataAnnotations;

namespace BLL.ModelVMs.Content;

public class AddBlog
{
    [Required]
    public string DoctorId { get; set; } = string.Empty;
    [Required]
    public string BlogContent { get; set; } = string.Empty;
}