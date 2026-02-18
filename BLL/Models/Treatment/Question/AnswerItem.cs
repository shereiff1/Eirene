

namespace BLL.Models.Treatment.Question;

public class AnswerItem
{
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
}
