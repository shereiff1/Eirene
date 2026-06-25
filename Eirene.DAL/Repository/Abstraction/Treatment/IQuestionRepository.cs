
using Eirene.DAL.Entities.Treatment;


namespace Eirene.DAL.Repository.Abstraction.Treatment;

public interface IQuestionRepository : IGenericRepository<Question>
{
    Task<Question?> GetByIdWithChoicesAsync(Guid id);
    Task<List<Question>> GetAllWithChoicesAsync();
}
