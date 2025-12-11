using DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
namespace DAL.Entities.Treatment;

public class QuestionAnswer
{
    public int Id { get; set; }
    public string Answer { get; set; } = string.Empty;
    public int QuestionId { get; set; }
    [ForeignKey(nameof(QuestionId))]
    public Question Question { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

}
