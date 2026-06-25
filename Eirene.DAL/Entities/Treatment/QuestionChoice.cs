using System.ComponentModel.DataAnnotations.Schema;

namespace Eirene.DAL.Entities.Treatment;

public class QuestionChoice
{
    public Guid Id { get; set; }
    public string ChoiceText { get; set; } = string.Empty;

    public Guid QuestionId { get; set; }
    [ForeignKey("QuestionId")]
    public Question Question { get; set; } = null!;
}
