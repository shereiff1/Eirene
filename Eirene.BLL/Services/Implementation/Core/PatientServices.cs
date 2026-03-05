using Eirene.DAL.Enumerators;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Eirene.BLL.Models.Core.Patient;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Identity;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class PatientServices : IPatientServices
    {
        private readonly IPatientProfileRepository _patientRepository;
        private readonly IDoctorProfileRepository _doctorRepository;
        private readonly ISupervisionRequestRepository _requestRepository;
        private readonly IDoctorRatingRepository _ratingRepository;
        private readonly IApplicationUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PatientServices> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public PatientServices(
            ILogger<PatientServices> logger,
            IMapper mapper,
            IPatientProfileRepository patientRepository,
            IDoctorProfileRepository doctorRepository,
            ISupervisionRequestRepository requestRepository,
            IDoctorRatingRepository ratingRepository,
            IApplicationUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _requestRepository = requestRepository;
            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _emailSender = emailSender;
        }

        public async Task<(bool IsSuccess, string? Error)> RequestSupervisionAsync(string patientUserId, string doctorId)
        {
            try
            {
                var patient = (await _patientRepository.GetAllAsync())
                    .FirstOrDefault(p => p.Id == patientUserId);

                if (patient == null)
                    return (false, "Patient profile not found. Please create a profile first.");

                if (patient.DoctorProfileId != null)
                    return (false, "You are already under a doctor's supervision.");

                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return (false, "Doctor not found.");

                var existingRequest = (await _requestRepository.FindAsync(
                        r => r.PatientProfileId == patient.Id &&
                             r.DoctorProfileId == doctorId &&
                             r.Status == SupervisionRequestStatus.Pending))
                    .FirstOrDefault();

                if (existingRequest != null)
                    return (false, "A pending request to this doctor already exists.");

                var request = new SupervisionRequest
                {
                    PatientProfileId = patient.Id,
                    DoctorProfileId = doctorId,
                    Status = SupervisionRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _requestRepository.AddAsync(request);
                await _unitOfWork.SaveChangesAsync();

                await _emailSender.SendEmailAsync($"{patient.User.Email}", "Supervision Request",
                    $"You sent a supervision request to Doctor {doctor.User.FullName}.");
                await _emailSender.SendEmailAsync($"{doctor.User.Email}", "Supervision Request",
                    $"A new patient supervision request has been assigned to you; please log in to your dashboard to review the details and respond at your earliest convenience.");
                
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supervision request for user {UserId}", patientUserId);
                return (false, "An error occurred while sending the supervision request.");
            }
        }

        public async Task<(bool IsSuccess, List<PatientModel>? Patients)> GetAllAsync()
        {
            try
            {
                var patients = await _patientRepository.GetAllAsync();
                if (patients == null)
                {
                    _logger.LogError("No patients found");
                    return (false, null);
                }

                var patientDtos = _mapper.Map<List<PatientModel>>(patients);
                _logger.LogInformation("Retrieved {Count} patients", patientDtos.Count);
                return (true, patientDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, PatientModel? Patient)> GetByIdAsync(string userId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(userId);
                if (patient == null)
                {
                    _logger.LogError("Patient with id {ID} not found", userId);
                    return (false, null);
                }
                var patientDto = _mapper.Map<PatientModel>(patient);
                _logger.LogInformation("Patient by id {ID} is found and retrieved.", userId);
                return (true, patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, string? Error, PatientModel? Patient)> CreatePatientProfileAsync(AddPatientProfile model, string userId)
        {
            try
            {
                var existingProfile = (await _patientRepository.GetAllAsync())
                    .FirstOrDefault(p => p.Id == userId);

                if (existingProfile != null)
                {
                    return (false, "Patient profile already exists for this user.", null);
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return (false, "User account not found.", null);

                user.PhoneNumber = model.PhoneNumber;
                await _userRepository.UpdateAsync(user);

                var patientEntity = _mapper.Map<PatientProfile>(model);
                patientEntity.Id = userId;

                await _patientRepository.AddAsync(patientEntity);
                await _unitOfWork.SaveChangesAsync();

                var createdPatient = await _patientRepository.GetByIdAsync(userId);
                var patientDto = _mapper.Map<PatientModel>(createdPatient);

                return (true, null, patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient profile for user {UserId}", userId);
                return (false, "An error occurred while creating the profile.", null);
            }
        }

        public async Task<(bool IsSuccess, string? Error, PatientModel? Patient)> UpdatePatientProfileAsync(EditPatientProfile model, string userId)
        {
            try
            {
                var existingProfile = await _patientRepository.GetByIdAsync(userId);
                var user = await _userRepository.GetByIdAsync(userId);
                if (existingProfile == null)
                {
                    return (false, "Patient profile not found.", null);
                }
                
                _mapper.Map(model, existingProfile);
                if (user == null)
                    return (false, "User account not found.", null);

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    user.PhoneNumber = model.PhoneNumber;
                    await _userRepository.UpdateAsync(user);
                }
                
                await _patientRepository.UpdateAsync(existingProfile);
                await _unitOfWork.SaveChangesAsync();

                var patientDto = _mapper.Map<PatientModel>(existingProfile);
                return (true, null, patientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient profile for user {UserId}", userId);
                return (false, "An error occurred while updating the profile.", null);
            }
        }

        public async Task<(bool IsSuccess, string? Error)> DeletePatientProfileAsync(string userId)
        {
            try
            {
                var existingProfile = await _patientRepository.GetByIdAsync(userId);

                if (existingProfile == null)
                {
                    return (false, "Patient profile not found.");
                }

                await _patientRepository.DeleteAsync(existingProfile);
                await _unitOfWork.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient profile for user {UserId}", userId);
                return (false, "An error occurred while deleting the profile.");
            }
        }

        public async Task<(bool IsSuccess, string? Error)> RemoveDoctorSupervision(string patientUserId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientUserId);
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
                var doctor = await _doctorRepository.GetByIdAsync(patient.DoctorProfileId);
                patient.DoctorProfileId = null;
                await _patientRepository.UpdateAsync(patient);
                
                var existingRequest = (await _requestRepository.FindAsync(
                        r => r.PatientProfileId == patient.Id &&
                             r.Status == SupervisionRequestStatus.Accepted))
                    .FirstOrDefault();
                await _requestRepository.DeleteAsync(existingRequest);
                await _unitOfWork.SaveChangesAsync();
                
                await _emailSender.SendEmailAsync($"{patient.User.Email}", "Supervision Canceled",
                    $"You removed the supervision request from Doctor {doctor.User.FullName}.");
                await _emailSender.SendEmailAsync($"{doctor.User.Email}", "Supervision Request",
                    $"Patient {patient.User.FullName}'s supervision has been canceled. Please log in to your dashboard to review the details and respond at your earliest convenience.");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing the supervision request for user {UserId}", patientUserId);
                return (false, "An error occurred while removing the supervision request.");
            }
        }
        public async Task<(bool IsSuccess, string? Error)> RateSupervisorAsync(string patientUserId, string doctorId, AddDoctorRatingDTO model)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientUserId);
                if (patient == null)
                    return (false, "Patient profile not found.");

                if (patient.DoctorProfileId != doctorId)
                    return (false, "You can only rate your assigned supervisor.");

                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return (false, "Doctor not found.");

                var existingRatings = await _ratingRepository.FindAsync(r => 
                    r.PatientProfileId == patientUserId && r.DoctorProfileId == doctorId);
                var existingRating = existingRatings.FirstOrDefault();

                if (existingRating != null)
                {
                    existingRating.Rating = model.Rating;
                    existingRating.Review = model.Review;
                    existingRating.UpdatedAt = DateTime.UtcNow;
                    await _ratingRepository.UpdateAsync(existingRating);
                }
                else
                {
                    var newRating = new DoctorRating
                    {
                        DoctorProfileId = doctorId,
                        PatientProfileId = patientUserId,
                        Rating = model.Rating,
                        Review = model.Review,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _ratingRepository.AddAsync(newRating);
                }
                await _unitOfWork.SaveChangesAsync();

                var allRatings = await _ratingRepository.FindAsync(r => r.DoctorProfileId == doctorId);
                doctor.ReviewCount = allRatings.Count();
                if (doctor.ReviewCount > 0)
                {
                     double avg = allRatings.Average(r => r.Rating);
                     doctor.Rating = Math.Round(avg, 1);
                }
                else
                {
                     doctor.Rating = 0;
                }
                
                await _doctorRepository.UpdateAsync(doctor);
                await _unitOfWork.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while patient {PatientId} rating doctor {DoctorId}", patientUserId, doctorId);
                return (false, "An error occurred while saving the rating.");
            }
        }
    }
}
