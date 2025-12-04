using AutoMapper;
using BLL.Models.Tracking;
using BLL.Services.Abstraction.Tracking;
using DAL.Entities.Tracking;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Tracking;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Tracking
{
    internal class JournalServices : IJournalServices
    {
        private readonly IJournalRepository _journalRepository;
        private readonly ILogger<JournalServices> _logger;
        private readonly IMapper _mapper;

        public JournalServices(IJournalRepository journalRepository, ILogger<JournalServices> logger, IMapper mapper)
        {
            _journalRepository = journalRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public Task<bool> CanCreateToday()
        {
            throw new NotImplementedException();
        }

        public async Task<(bool IsSuccess, JournalDTO? AddedJournal)> CreateAsync(AddJournal model)
        {
            try
            {
                var journalEntity = _mapper.Map<Journal>(model);

                var result = await _journalRepository.AddAsync(journalEntity);

                if (result == null)
                {
                    return (false, null);
                }

                var addedDto = _mapper.Map<JournalDTO>(journalEntity);

                return (true, addedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a journal entry.");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<JournalDTO>? journals)> GetAllAsync()
        {
            try
            {
                var journalEntities = await _journalRepository.GetAllAsync();
                if (journalEntities == null || !journalEntities.Any())
                {
                    return (false, null);
                }
                var journalDtos = _mapper.Map<List<JournalDTO>>(journalEntities);
                return (true, journalDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all journal entries.");
                return (false, null);

            }
        }
        public async Task<(bool IsSuccess, JournalDTO? journal)> GetByIdAsync(int id)
        {
            try
            {
                var journalEntity = await _journalRepository.GetByIdAsync(id);
                if (journalEntity == null)
                {
                    return (false, null);
                }
                var journalDto = _mapper.Map<JournalDTO>(journalEntity);
                return (true, journalDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving a journal entry by ID.");
                return (false, null);

            }
        }
        public async Task<bool> UpdateAsync(int id, EditJournal model)
        {
            try
            {
                var journalEntity = await _journalRepository.GetByIdAsync(id);
                if (journalEntity == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating a journal entry.");
                return false;
            }
        }

        private string GetCurrentUserId()
        {

            throw new NotImplementedException("Implement GetCurrentUserId");
        }
    }
}