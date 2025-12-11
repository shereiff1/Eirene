using BLL.ModelVMs.Treatment;

namespace BLL.Services.Abstraction.Treatment;

public interface IQuestionServices
{
    Task<(bool IsSuccess, List<QuestionDTO>? questions)> GetAllAsync();
    Task<(bool IsSuccess, QuestionDTO? question)> GetByIdAsync(int id);
    Task<(bool IsSuccess, QuestionDTO? AddedQuestion)> CreateAsync(AddQuestion model);

    Task<(bool IsSuccess, EditQuestion? editedQuestion)> UpdateAsync(EditQuestion model);
    Task<bool> DeleteAsync(int id);

}
