using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Models.Core.Doctor;
using BLL.Services.Abstraction.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Core
{
    public class DoctorServices :  IDoctorServices
    {
        private readonly ILogger<DoctorServices> _logger;
        private readonly IMapper _mapper;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public DoctorServices(ILogger<DoctorServices> logger, IMapper mapper, IDoctorProfileRepository doctorProfileRepository, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsSuccess, List<DoctorModel>? Doctors)> GetAllAsync()
        {
            try
            {
                var doctors = await _doctorProfileRepository.GetAllAsync();
                if (doctors == null)
                {
                    _logger.LogError("No doctors found");
                    return (false, null);
                }
                var doctorDtos = _mapper.Map<List<DoctorModel>>(doctors);

                _logger.LogInformation("Retrieved {Count} doctors", doctorDtos.Count);
                return (true, doctorDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool isSuccess, DoctorModel? Doctor)> GetByIdAsync(string id)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(id);
                if (doctor == null)
                {
                    _logger.LogError("No doctors found");
                    return (false, null);
                }

                var doctorDto = _mapper.Map<DoctorModel>(doctor);
                _logger.LogInformation("Doctor by id {ID} is Found and retrieved.", id);
                return (true, doctorDto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, string? Error, DoctorModel? Doctor)> CreateDoctorProfileAsync(AddDoctorProfile model, string userId)
        {
            try
            {
                var existingProfile = (await _doctorProfileRepository.GetAllAsync())
                    .FirstOrDefault(d => d.UserId == userId);

                if (existingProfile != null)
                {
                    return (false, "Doctor profile already exists for this user.", null);
                }

                var doctorEntity = _mapper.Map<DAL.Entities.Core.DoctorProfile>(model);
                doctorEntity.UserId = userId;
                doctorEntity.JoinedAt = DateTime.UtcNow;

                await _doctorProfileRepository.AddAsync(doctorEntity);
                await _unitOfWork.SaveChangesAsync();
                var createdDoctor = await _doctorProfileRepository.GetByIdAsync(doctorEntity.UserId);
                var doctorDto = _mapper.Map<DoctorModel>(createdDoctor);

                return (true, null, doctorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor profile for user {UserId}", userId);
                return (false, "An error occurred while creating the profile.", null);
            }
        }

        public async Task<(bool IsSuccess, string? Error, DoctorModel? Doctor)> UpdateDoctorProfileAsync(EditDoctorProfile model, string userId)
        {
            try
            {
                var existingProfile = (await _doctorProfileRepository.GetAllAsync())
                    .FirstOrDefault(d => d.UserId == userId);

                if (existingProfile == null)
                {
                    return (false, "Doctor profile not found.", null);
                }

                _mapper.Map(model, existingProfile);
                existingProfile.UpdatedAt = DateTime.UtcNow;

                await _doctorProfileRepository.UpdateAsync(existingProfile); 
                await _unitOfWork.SaveChangesAsync();

                var doctorDto = _mapper.Map<DoctorModel>(existingProfile);
                return (true, null, doctorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor profile for user {UserId}", userId);
                return (false, "An error occurred while updating the profile.", null);
            }
        }
    }
}
