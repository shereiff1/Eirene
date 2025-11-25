
using DAL.Entities.Tracking;
using DAL.Repository.Abstraction.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Implementation.Tracking
{
    public class JournalRepository : GenericRepository<Journal>, IJournalRepository
    {
    }
}
