using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class SupervisionService : ISupervisionService
    {
        private readonly ILogger<SupervisionService> _logger;
        private readonly ISupervisionRequestRepository _requestRepository;
        private readonly IPatientProfileRepository _patientProfileRepository;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IMapper _mapper;

        public SupervisionService(
            ILogger<SupervisionService> logger,
            ISupervisionRequestRepository requestRepository,
            IPatientProfileRepository patientProfileRepository,
            IDoctorProfileRepository doctorProfileRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IBackgroundJobService backgroundJobService,
            IMapper mapper)
        {
            _logger = logger;
            _requestRepository = requestRepository;
            _patientProfileRepository = patientProfileRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _backgroundJobService = backgroundJobService;
            _mapper = mapper;
        }

        public async Task<Result> RespondToSupervisionRequestAsync(string requestId, bool accept, string doctorUserId)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                    return Result.Failure("Supervision request not found.");

                if (request.DoctorProfileId != doctorUserId)
                    return Result.Failure("You are not authorized to respond to this request.");

                if (request.Status != SupervisionRequestStatus.Pending)
                    return Result.Failure("This request has already been responded to.");

                if (accept)
                    request.Accept();
                else
                    request.Decline();

                if (accept)
                {
                    var patient = await _patientProfileRepository.GetByIdAsync(request.PatientProfileId);
                    if (patient == null)
                        return Result.Failure("Patient profile not found.");

                    patient.DoctorProfileId = doctorUserId;
                    await _patientProfileRepository.UpdateAsync(patient);
                    var doctor = await _doctorProfileRepository.GetByIdAsync(doctorUserId);
                    var doctorFullName = doctor?.User?.FullName ?? string.Empty;
                    var patientFullName = patient?.User?.FullName ?? string.Empty;
                    
                    _backgroundJobService.Enqueue(() => _emailSender.SendEmailAsync(patient.User.Email, "Supervision Request Update", $"Your supervision request to Doctor {doctorFullName} has been accepted."));
                    _backgroundJobService.Enqueue(() => _emailSender.SendEmailAsync(doctor.User.Email, "Supervision Update", $"You are now {patientFullName}'s Supervisor."));

                    var otherRequests = await _requestRepository.FindAsync(
                        r => r.PatientProfileId == request.PatientProfileId &&
                             r.Id != requestId &&
                             r.Status == SupervisionRequestStatus.Pending);

                    foreach (var other in otherRequests)
                        await _requestRepository.DeleteAsync(other);
                }

                await _requestRepository.UpdateAsync(request);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to supervision request {RequestId}", requestId);
                return Result.Failure("An error occurred while responding to the request.");
            }
        }

        public async Task<Result<List<SupervisionRequestDTO>>> GetSupervisionRequestsAsync(string doctorUserId)
        {
            try
            {
                var requests = await _requestRepository.GetRequestsByDoctorIdAsync(doctorUserId);
                var dtos = _mapper.Map<List<SupervisionRequestDTO>>(requests);

                return Result.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supervision requests for doctor {DoctorProfileId}", doctorUserId);
                return Result.Failure<List<SupervisionRequestDTO>>("An error occurred while fetching supervision requests.");
            }
        }

        public async Task<Result<List<DoctorPatientDTO>>> GetDoctorsPatientsAsync(string doctorUserId)
        {
            try
            {
                var requests = await _requestRepository.GetDoctorPatientsAsync(doctorUserId);
                var models = _mapper.Map<List<DoctorPatientDTO>>(requests);

                return Result.Success(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor's patients for doctor {DoctorProfileId}", doctorUserId);
                return Result.Failure<List<DoctorPatientDTO>>("An error occurred while fetching patients.");
            }
        }

        public async Task<Result> RemoveSupervisionOnPatient(string patientUserId)
        {
            try
            {
                var patient = await _patientProfileRepository.GetByIdAsync(patientUserId);
                if (patient == null)
                {
                    _logger.LogError("Patient profile not found for user {UserId}", patientUserId);
                    return Result.Failure("Patient profile not found.");
                }

                if (patient.DoctorProfileId == null)
                {
                    _logger.LogWarning("Patient {UserId} is not under a doctor's supervision.", patientUserId);
                    return Result.Success();
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
                    _backgroundJobService.Enqueue(() => _emailSender.SendEmailAsync(patientEmail, "Supervision Canceled",
                        $"You removed the supervision request from Doctor {doctorFullName}."));
                }

                if (!string.IsNullOrEmpty(doctorEmail))
                {
                    _backgroundJobService.Enqueue(() => _emailSender.SendEmailAsync(doctorEmail, "Supervision Canceled", $"Patient {patientFullName}'s supervision has been canceled. Please log in to your dashboard to review the details and respond at your earliest convenience."));
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing the supervision request for user {UserId}", patientUserId);
                return Result.Failure("An error occurred while removing the supervision request.");
            }
        }
    }
}
