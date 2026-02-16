namespace BLL.ModelVMs.Content;

public class BlogDTO
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string BlogContent { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}