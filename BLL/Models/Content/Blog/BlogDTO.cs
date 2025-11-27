namespace BLL.ModelVMs.Content;

public class BlogDTO
{
    public int Id { get; set; }
    public string DoctorId { get; set; }
    public string BlogContent { get; set; }
    public DateTime CreatedAt { get; set; }
}