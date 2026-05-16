using Eirene.DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;
namespace Eirene.DAL.Entities.Treatment;

public class QuestionAnswer
{
    public Guid Id { get; set; }
    public string Answer { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    [ForeignKey("QuestionId")]
    public Question Question { get; set; } = null!;
    public string PatientId { get; set; } = string.Empty;
    [ForeignKey("PatientId")]
    public ApplicationUser User { get; set; } = null!;

}
