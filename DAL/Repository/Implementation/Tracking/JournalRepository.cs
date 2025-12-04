using DAL.Database;
using DAL.Entities.Tracking;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Tracking;
using Microsoft.EntityFrameworkCore;


namespace DAL.Repository.Implementation.Tracking;

internal class JournalRepository : GenericRepository<Journal>, IJournalRepository
{
    public JournalRepository(EireneDBContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}