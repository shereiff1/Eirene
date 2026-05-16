using System;

namespace Eirene.BLL.Models.Treatment.Task;

public class PatientTaskResponseDTO
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PatientId { get; set; } = string.Empty;
}
