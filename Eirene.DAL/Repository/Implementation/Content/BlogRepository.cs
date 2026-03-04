using DAL.Entities.Content;
using DAL.Repository.Abstraction.Content;
using DAL.Database;
using DAL.Repository.Abstraction;

namespace DAL.Repository.Implementation.Content
{
    public class BlogRepository: GenericRepository<Blog>, IBlogRepository
    {
        public BlogRepository(EireneDBContext context) : base(context)
        {
        }
    }
}
