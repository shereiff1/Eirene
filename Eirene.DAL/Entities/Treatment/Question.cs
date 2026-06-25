

namespace Eirene.DAL.Entities.Treatment;

public class Question
{
    public Guid Id { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public ICollection<QuestionChoice> Choices { get; set; } = new List<QuestionChoice>();
}
