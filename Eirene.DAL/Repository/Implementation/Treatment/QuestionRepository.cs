using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Eirene.DAL.Database;

namespace Eirene.DAL.Repository.Implementation.Treatment;

internal class QuestionRepository : GenericRepository<Question>, IQuestionRepository
{
    public QuestionRepository(EireneDBContext context) : base(context)
    {
    }
}
