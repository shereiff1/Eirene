using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.ModelVMs.Content;

public class AddBlog
{
    [Required]
    public string BlogContent { get; set; } = string.Empty;
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Topic { get; set; } = string.Empty;
}