using Eirene.BLL.Models.Tracking;

namespace Eirene.BLL.Services.Abstraction.Tracking
{
    public interface IJournalServices
    {
        Task<(bool IsSuccess, List<JournalDTO>? journals)> GetAllAsync();

        Task<(bool IsSuccess, JournalDTO? journal)> GetByIdAsync(Guid id);

        Task<(bool IsSuccess, JournalDTO? AddedJournal)> CreateAsync(AddJournal model);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UpdateAsync(EditJournal model);
        Task<bool> CanCreateToday();
    }
}
