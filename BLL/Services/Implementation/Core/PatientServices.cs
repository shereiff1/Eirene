using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Models.Core.Patient;
using BLL.Services.Abstraction.Core;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Core
{
    public class PatientServices : IPatientServices
    {
        private readonly IPatientProfileRepository _patientRepository;
        private readonly IDoctorProfileRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PatientServices> _logger;
        private readonly IMapper _mapper;

        public PatientServices(
            ILogger<PatientServices> logger, 
            IMapper mapper,
            IPatientProfileRepository patientRepository,
            IDoctorProfileRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<(bool IsSuccess, string? Error)> AssignDoctorAsync(string patientUserId, string doctorId)
        {
            try
            {
                var patient = (await _patientRepository.GetAllAsync())
                    .FirstOrDefault(p => p.UserId == patientUserId);

                if (patient == null)
                    return (false, "Patient profile not found. Please create a profile first.");

                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return (false, "Doctor not found.");

                patient.DoctorProfileId = doctorId;
                await _patientRepository.UpdateAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "An error occurred while assigning the doctor.");
            }
        }

        // public async Task<(bool IsSuccess, string? Error)> CreatePatientProfileAsync(AddPatientProfile model, string patientId)
        // {
        //     try
        //     {
        //         var existingProfile = (await _patientRepository.GetAllAsync())
        //             .FirstOrDefault(d => d.UserId == patientId);
        //
        //         if (existingProfile != null)
        //         {
        //             return (false, "Doctor profile already exists for this user.", null);
        //         }
        //
        //         var patientEntity = _mapper.Map<PatientProfile>(model);
        //         patientEntity.UserId = patientId;
        //         patientEntity.CreatedAt = DateTime.UtcNow;
        //
        //         await _patientRepository.AddAsync(patientEntity);
        //         await _unitOfWork.SaveChangesAsync();
        //         var createdDoctor = await _patientRepository.GetByIdAsync(patientEntity.UserId);
        //         var doctorDto = _mapper.Map<PatientModel>(createdDoctor);
        //
        //         return (true, null, doctorDto);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error creating doctor profile for user {UserId}", userId);
        //         return (false, "An error occurred while creating the profile.", null);
        //     }
        // }
        //
    }
}
