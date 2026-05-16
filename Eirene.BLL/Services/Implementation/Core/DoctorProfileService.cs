using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly ILogger<DoctorProfileService> _logger;
        private readonly IMapper _mapper;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorProfileService(
            ILogger<DoctorProfileService> logger,
            IMapper mapper,
            IDoctorProfileRepository doctorProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWork = unitOfWork;
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
                var doctor = await _doctorProfileRepository.GetByIdAsync(id);
                if (doctor == null)
                {
                    _logger.LogError("No doctors found");
                    return Result.Failure<DoctorModel>("Doctor not found.");
                }

                var doctorDto = _mapper.Map<DoctorModel>(doctor);
                _logger.LogInformation("Doctor by id {ID} is Found and retrieved.", id);
                return Result.Success(doctorDto);
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
                    return Result.Failure<DoctorModel>("Doctor profile already exists for this user.");
                }

                var doctorEntity = _mapper.Map<DoctorProfile>(model);
                doctorEntity.Id = userId;
                doctorEntity.JoinedAt = DateTime.UtcNow;

                await _doctorProfileRepository.AddAsync(doctorEntity);
                await _unitOfWork.SaveChangesAsync();
                var createdDoctor = await _doctorProfileRepository.GetByIdAsync(doctorEntity.Id);
                var doctorDto = _mapper.Map<DoctorModel>(createdDoctor);

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
                    return Result.Failure<DoctorModel>("Doctor profile not found.");
                }

                _mapper.Map(model, existingProfile);
                existingProfile.Update();

                await _doctorProfileRepository.UpdateAsync(existingProfile);
                await _unitOfWork.SaveChangesAsync();

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
                    return Result.Failure("Doctor profile not found.");
                }
                await _doctorProfileRepository.DeleteAsync(doctor);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor profile for user {UserId}", doctorId);
                return Result.Failure("An error occurred while deleting the profile.");
            }
        }

        public async Task<Result<bool>> CheckIfVerified(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return Result.Failure<bool>("Doctor Not Found");
                }
                return Result.Success(doctor.IsVerified);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for doctor {DoctorId}", doctorId);
                return Result.Failure<bool>("An error happened while fetching doctor data");
            }
        }
    }
}
