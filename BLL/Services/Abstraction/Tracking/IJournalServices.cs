using BLL.Models.Tracking;

namespace BLL.Services.Abstraction.Tracking
{
    public interface IJournalServices
    {
        Task<(bool IsSuccess, List<JournalDTO>? journals)> GetAllAsync();

        Task<(bool IsSuccess, JournalDTO? journal)> GetByIdAsync(int id);

        Task<(bool IsSuccess, JournalDTO? AddedJournal)> CreateAsync(AddJournal model);

        Task<bool> UpdateAsync(int id, EditJournal model);
        Task<bool> CanCreateToday();
    }
}
