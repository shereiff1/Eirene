using AutoMapper;
using BLL.Models.Tracking;
using BLL.Services.Abstraction.Tracking;
using DAL.Entities.Tracking;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Tracking;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Tracking
{
    public class JournalServices : IJournalServices
    {
        private readonly IJournalRepository _journalRepository;
        private readonly ILogger<JournalServices> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public JournalServices(IJournalRepository journalRepository, ILogger<JournalServices> logger, IMapper mapper,
            IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork
        )
        {
            _journalRepository = journalRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CanCreateToday()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;
            var existingJournal = await _journalRepository.GetTodayJournalAsync(userId, today);
            return existingJournal == null;
        }

        public async Task<(bool IsSuccess, JournalDTO? AddedJournal)> CreateAsync(AddJournal model)
        {
            try
            {
                var journalEntity = _mapper.Map<Journal>(model);
                journalEntity.PatientId = GetCurrentUserId();
                if (!await CanCreateToday())
                {
                    _logger.LogError("The patient has already created a journal today");
                    return (false, null);
                }

                var result = await _journalRepository.AddAsync(journalEntity);
                await _unitOfWork.SaveChangesAsync();

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
                var userId = GetCurrentUserId();
                var journalEntities = await _journalRepository.GetAllForUserAsync(userId);
                if (!journalEntities.Any())
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

        public async Task<bool> UpdateAsync(EditJournal model)
        {
            try
            {
                var userId = GetCurrentUserId();

                var journal = await _journalRepository.GetByIdAsync(model.Id);

                if (journal == null)
                    return false;

                if (journal.PatientId != userId)
                    return false;

                if (journal.CreatedAt.Date != DateTime.UtcNow.Date)
                    return false;

                journal.Content = model.Content;

                var result = await _journalRepository.UpdateAsync(journal);
                await _unitOfWork.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating a journal entry.");
                return false;
            }
        }

        private string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                throw new Exception("User is not authenticated.");

            return user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        }
    }
}
