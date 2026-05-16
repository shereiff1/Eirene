using Eirene.DAL.Entities.Content;
using Eirene.DAL.Repository.Abstraction.Content;
using Eirene.DAL.Database;
using Eirene.DAL.Repository.Abstraction;

namespace Eirene.DAL.Repository.Implementation.Content
{
    public class BlogRepository: GenericRepository<Blog>, IBlogRepository
    {
        public BlogRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
