using DAL.Entities.Community;
using DAL.Entities.Treatment;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Treatment;

using DAL.Database;
using DAL.Repository.Abstraction;


namespace DAL.Repository.Implementation.Treatment;

public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
{
    public QuestionRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}
