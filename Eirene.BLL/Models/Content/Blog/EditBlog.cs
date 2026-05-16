namespace Eirene.BLL.ModelVMs.Content;

public class EditBlog
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string BlogContent { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
