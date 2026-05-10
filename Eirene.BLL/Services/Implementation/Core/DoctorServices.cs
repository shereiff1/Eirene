using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.DAL.Enumerators;
using Eirene.BLL.Models.Core;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class DoctorServices :  IDoctorServices
    {
        private readonly ILogger<DoctorServices> _logger;
        private readonly IMapper _mapper;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IPatientProfileRepository _patientProfileRepository;
        private readonly ISupervisionRequestRepository _requestRepository;
        private readonly IDoctorRatingRepository _ratingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IBackgroundJobService _backgroundJobService;
        
        public DoctorServices(
            ILogger<DoctorServices> logger,
            IMapper mapper,
            IDoctorProfileRepository doctorProfileRepository,
            IPatientProfileRepository patientProfileRepository,
            ISupervisionRequestRepository requestRepository,
            IDoctorRatingRepository ratingRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IBackgroundJobService backgroundJobService)
        {
            _logger = logger;
            _mapper = mapper;
            _doctorProfileRepository = doctorProfileRepository;
            _patientProfileRepository = patientProfileRepository;
            _requestRepository = requestRepository;
            _ratingRepository = ratingRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _backgroundJobService = backgroundJobService;
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
                    .FirstOrDefault(d => d.Id == userId);

                if (existingProfile != null)
                {
                    return (false, "Doctor profile already exists for this user.", null);
                }

                var doctorEntity = _mapper.Map<DoctorProfile>(model);
                doctorEntity.Id = userId;
                doctorEntity.JoinedAt = DateTime.UtcNow;

                await _doctorProfileRepository.AddAsync(doctorEntity);
                await _unitOfWork.SaveChangesAsync();
                var createdDoctor = await _doctorProfileRepository.GetByIdAsync(doctorEntity.Id);
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
                    .FirstOrDefault(d => d.Id == userId);

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
        public async Task<(bool IsSuccess, string? Error)> RespondToSupervisionRequestAsync(string requestId, bool accept, string doctorUserId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                    return (false, "Supervision request not found.");
                
                if (request.DoctorProfileId != doctorUserId)
                    return (false, "You are not authorized to respond to this request.");
                
                if (request.Status != SupervisionRequestStatus.Pending)
                    return (false, "This request has already been responded to.");
                
                request.Status = accept ? SupervisionRequestStatus.Accepted : SupervisionRequestStatus.Declined;
                request.RespondedAt = DateTime.UtcNow;

                if (accept)
                {
                    var patient = await _patientProfileRepository.GetByIdAsync(request.PatientProfileId);
                    if (patient == null)
                        return (false, "Patient profile not found.");
                    
                    patient.DoctorProfileId = doctorUserId;
                    await _patientProfileRepository.UpdateAsync(patient);
                    var doctor = await _doctorProfileRepository.GetByIdAsync(doctorUserId);
                    var doctorFullName = doctor?.User?.FullName ?? string.Empty;
                    var patientFullName = patient?.User?.FullName ?? string.Empty;
                    _backgroundJobService.Enqueue(()=>_emailSender.SendEmailAsync(patient.User.Email, "Supervision Request Update", $"Your supervision request to Doctor {doctorFullName} has been accepted."));
                    // await _emailSender.SendEmailAsync(patient.User.Email, "Supervision Request Update", $"Your supervision request to Doctor {doctorFullName} has been accepted.");
                    _backgroundJobService.Enqueue(()=>_emailSender.SendEmailAsync(doctor.User.Email, "Supervision Update", $"You are now {patientFullName}'s Supervisor."));
                    // await _emailSender.SendEmailAsync(doctor.User.Email, "Supervision Update", $"You are now {patientFullName}'s Supervisor.");
                    var otherRequests = await _requestRepository.FindAsync(
                        r => r.PatientProfileId == request.PatientProfileId &&
                             r.Id != requestId &&
                             r.Status == SupervisionRequestStatus.Pending);

                    foreach (var other in otherRequests)
                        await _requestRepository.DeleteAsync(other);
                }

                await _requestRepository.UpdateAsync(request);
                await _unitOfWork.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to supervision request {RequestId}", requestId);
                return (false, "An error occurred while responding to the request.");
            }
        }

        public async Task<(bool IsSuccess, List<SupervisionRequest>? Requests)> GetSupervisionRequestsAsync(string doctorUserId)
        {
            try
            {
                var requests = await _requestRepository.FindAsync(
                    r => r.DoctorProfileId == doctorUserId);

                var models = requests.Select(r => new SupervisionRequest
                {
                    Id = r.Id,
                    PatientProfileId = r.PatientProfileId,
                    DoctorProfileId = r.DoctorProfileId,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    RespondedAt = r.RespondedAt
                }).ToList();

                return (true, models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervision requests for doctor {DoctorProfileId}", doctorUserId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<DoctorPatientDTO>? Patients)> GetDoctorsPatientsAsync(string doctorUserId)
        {
            try
            {
                var requests = await _requestRepository.GetDoctorPatientsAsync(doctorUserId);

                var models = requests.Select(r => new DoctorPatientDTO
                {
                    RequestId = r.Id,
                    PatientId = r.PatientProfileId,
                    FullName = r.Patient?.User?.FullName ?? "Unknown",
                    Email = r.Patient?.User?.Email ?? "Unknown",
                    DateOfBirth = r.Patient?.DateOfBirth ?? DateTime.MinValue,
                    ProfilePhotoUrl = r.Patient?.ProfilePhotoUrl,
                    AcceptedAt = r.RespondedAt ?? r.CreatedAt
                }).ToList();

                return (true, models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor's patients for doctor {DoctorProfileId}", doctorUserId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, string? Error)> RemoveSupervisionOnPatient(string patientUserId)
        {
            try
            {
                var patient = await _patientProfileRepository.GetByIdAsync(patientUserId);
                if (patient == null)
                {
                    _logger.LogError("Patient profile not found for user {UserId}", patientUserId);
                    return (false, "Patient profile not found.");
                }

                if (patient.DoctorProfileId == null)
                {
                    _logger.LogWarning("Patient {UserId} is not under a doctor's supervision.", patientUserId);
                    return (true, null);
                }
                
                var doctor = await _doctorProfileRepository.GetByIdAsync(patient.DoctorProfileId);
                patient.DoctorProfileId = null;
                await _patientProfileRepository.UpdateAsync(patient);
                
                var existingRequest = (await _requestRepository.FindAsync(
                        r => r.PatientProfileId == patient.Id &&
                             r.Status == SupervisionRequestStatus.Accepted))
                    .FirstOrDefault();
                
                if (existingRequest != null)
                {
                    await _requestRepository.DeleteAsync(existingRequest);
                }
                await _unitOfWork.SaveChangesAsync();
                
                var patientEmail = patient.User?.Email;
                var doctorEmail = doctor?.User?.Email;
                var patientFullName = patient.User?.FullName ?? string.Empty;
                var doctorFullName = doctor?.User?.FullName ?? string.Empty;

                if (!string.IsNullOrEmpty(patientEmail))
                {
                    _backgroundJobService.Enqueue(()=>_emailSender.SendEmailAsync(patientEmail, "Supervision Canceled",
                        $"You removed the supervision request from Doctor {doctorFullName}."));
                    // await _emailSender.SendEmailAsync(patientEmail, "Supervision Canceled",
                    //     $"You removed the supervision request from Doctor {doctorFullName}.");
                }
                
                if (!string.IsNullOrEmpty(doctorEmail))
                {
                    _backgroundJobService.Enqueue(()=>_emailSender.SendEmailAsync(doctorEmail, "Supervision Canceled", $"Patient {patientFullName}'s supervision has been canceled. Please log in to your dashboard to review the details and respond at your earliest convenience."));
                    // await _emailSender.SendEmailAsync(doctorEmail, "Supervision Canceled",
                    //     $"Patient {patientFullName}'s supervision has been canceled. Please log in to your dashboard to review the details and respond at your earliest convenience.");
                }
                
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing the supervision request for user {UserId}", patientUserId);
                return (false, "An error occurred while removing the supervision request.");
            }
        }

        public async Task<(bool IsSuccess, List<DoctorRatingDTO>? Ratings)> GetDoctorRatingsAsync(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return (false, null);
                }

                var ratings = await _ratingRepository.FindAsync(r => r.DoctorProfileId == doctorId);
                
                var ratingDtos = new List<DoctorRatingDTO>();
                foreach (var r in ratings)
                {
                    var patient = await _patientProfileRepository.GetByIdAsync(r.PatientProfileId);
                    ratingDtos.Add(new DoctorRatingDTO
                    {
                        Id = r.Id,
                        PatientProfileId = r.PatientProfileId,
                        PatientName = patient?.User?.FullName ?? "Unknown",
                        Rating = r.Rating,
                        Review = r.Review,
                        CreatedAt = r.CreatedAt
                    });
                }
                
                return (true, ratingDtos.OrderByDescending(r => r.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ratings for doctor {DoctorId}", doctorId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, string? Error, bool IsVerified)> CheckIfVerified(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return (false, "Doctor Not Found", false);
                }
                return (true, null, doctor.IsVerified);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for doctor {DoctorId}", doctorId);
                return (false, "An error happened while fetching doctor data", false);
            }
        }

        public async Task<(bool IsSuccess, string? Error)> DeleteDoctorProfile(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return (false, null);
                }
                await _doctorProfileRepository.DeleteAsync(doctor);
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor profile for user {UserId}", doctorId);
                return (false, "An error occurred while deleting the profile.");
            }
        }
    }
}
