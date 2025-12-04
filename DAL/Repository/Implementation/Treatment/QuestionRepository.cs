using DAL.Entities.Community;
using DAL.Entities.Treatment;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Treatment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Database;
using DAL.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository.Implementation.Treatment
{
    internal class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        public QuestionRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
        }
    }
}
