using DAL.Entities.Content;
using DAL.Entities.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstraction.Tracking
{
    public interface IJournalRepository : IGenericRepository<Journal>
    {
        Task<Journal?> GetTodayJournalAsync(string userId, DateTime date);
        Task<List<Journal>?> GetAllForUserAsync(string userId);
    }
}