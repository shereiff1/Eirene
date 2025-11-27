using System.ComponentModel.DataAnnotations;

namespace BLL.ModelVMs.Content;

public class AddBlog
{
    [Required] public string DoctorId { get; set; }
    [Required] public string BlogContent { get; set; }
}