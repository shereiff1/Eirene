using Eirene.DAL.Entities.Community;
using Eirene.DAL.Entities.Treatment;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Repository.Abstraction.Treatment;

using Eirene.DAL.Database;
using Eirene.DAL.Repository.Abstraction;


namespace Eirene.DAL.Repository.Implementation.Treatment;

public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
{
    public QuestionRepository(EireneDBContext context) : base(context)
    {
    }
}
