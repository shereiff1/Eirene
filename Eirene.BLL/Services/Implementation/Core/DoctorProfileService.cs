using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly ILogger<DoctorProfileService> _logger;
        private readonly IMapper _mapper;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly HybridCache _cache;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorProfileService(
            ILogger<DoctorProfileService> logger,
            IMapper mapper,
            IDoctorProfileRepository doctorProfileRepository,
            HybridCache cache,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<Result<List<DoctorModel>>> GetAllAsync()
        {
            try
            {
                var doctors = await _doctorProfileRepository.GetAllAsync();
                if (doctors == null)
                {
                    _logger.LogError("No doctors found");
                    return Result.Failure<List<DoctorModel>>("No doctors found");
                }
                var doctorDtos = _mapper.Map<List<DoctorModel>>(doctors);

                _logger.LogInformation("Retrieved {Count} doctors", doctorDtos.Count);
                return Result.Success(doctorDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result.Failure<List<DoctorModel>>("An error occurred while retrieving doctors.");
            }
        }

        public async Task<Result<DoctorModel>> GetByIdAsync(string id)
        {
            try
            {
                var doctorModel = await _cache.GetOrCreateAsync(
                    key: $"doctor:{id}",
                    factory: async (ct) =>
                    {
                        _logger.LogInformation("Cache miss for doctor {id}", id);
                        var doctor = await _doctorProfileRepository.GetByIdAsync(id);
                        if (doctor is null)
                        {
                            return null;
                        }
                        return _mapper.Map<DoctorModel>(doctor);
                    },
                    options: new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(10)
                    },
                    cancellationToken: CancellationToken.None
                );
                if (doctorModel == null)
                {
                    _logger.LogError("No doctors found");
                    return Result.Failure<DoctorModel>("Doctor not found.");
                }

                _logger.LogInformation("Doctor by id {ID} is Found and retrieved.", id);
                return Result.Success(doctorModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result.Failure<DoctorModel>("An error occurred while retrieving the doctor.");
            }
        }

        public async Task<Result<DoctorModel>> CreateDoctorProfileAsync(AddDoctorProfile model, string userId)
        {
            try
            {
                var existingProfile = (await _doctorProfileRepository.GetAllAsync())
                    .FirstOrDefault(d => d.Id == userId);

                if (existingProfile != null)
                {
                    _logger.LogError("Doctor profile already exists for user {UserId}.", userId);
                    return Result.Failure<DoctorModel>("Doctor profile already exists for this user.");
                }

                var doctorEntity = _mapper.Map<DoctorProfile>(model);
                doctorEntity.Id = userId;
                doctorEntity.JoinedAt = DateTime.UtcNow;

                await _doctorProfileRepository.AddAsync(doctorEntity);
                await _unitOfWork.SaveChangesAsync();
                await _cache.RemoveAsync($"doctor:{userId}");
                var createdDoctor = await _doctorProfileRepository.GetByIdAsync(doctorEntity.Id);
                var doctorDto = _mapper.Map<DoctorModel>(createdDoctor);
                _logger.LogInformation("Created doctor profile for user {UserId}.", userId);
                return Result.Success(doctorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor profile for user {UserId}", userId);
                return Result.Failure<DoctorModel>("An error occurred while creating the profile.");
            }
        }

        public async Task<Result<DoctorModel>> UpdateDoctorProfileAsync(EditDoctorProfile model, string userId)
        {
            try
            {
                var existingProfile = (await _doctorProfileRepository.GetAllAsync())
                    .FirstOrDefault(d => d.Id == userId);
                if (existingProfile == null)
                {
                    _logger.LogError("Doctor profile not found for user {UserId}.", userId);
                    return Result.Failure<DoctorModel>("Doctor profile not found.");
                }

                _mapper.Map(model, existingProfile);
                existingProfile.Update();
                await _doctorProfileRepository.UpdateAsync(existingProfile);
                await _cache.RemoveAsync($"doctor:{userId}");
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated doctor profile for user {UserId}.", userId);

                var doctorDto = _mapper.Map<DoctorModel>(existingProfile);
                return Result.Success(doctorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor profile for user {UserId}", userId);
                return Result.Failure<DoctorModel>("An error occurred while updating the profile.");
            }
        }

        public async Task<Result> DeleteDoctorProfile(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogError("Doctor profile not found for user {UserId}.", doctorId);
                    return Result.Failure("Doctor profile not found.");
                }
                await _doctorProfileRepository.DeleteAsync(doctor);
                await _cache.RemoveAsync($"doctor:{doctorId}");
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Deleted doctor profile for user {UserId}.", doctorId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor profile for user {UserId}", doctorId);
                return Result.Failure("An error occurred while deleting the profile.");
            }
        }

        public async Task<Result<VerificationStatus>> CheckVerificationStatus(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor profile not found for user {UserId}.", doctorId);
                    return Result.Failure<VerificationStatus>("Doctor not found.");
                }

                if (doctor.DoctorVerification == null)
                {
                    _logger.LogWarning("Verification data for doctor {DoctorId} not found.", doctorId);
                    return Result.Failure<VerificationStatus>("Doctor verification data not found.");
                }

                return Result.Success(doctor.DoctorVerification.VerificationStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching verification status for doctor {DoctorId}", doctorId);
                return Result.Failure<VerificationStatus>("An error occurred while fetching verification status.");
            }
        }
    }
}
