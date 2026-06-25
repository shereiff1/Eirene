using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Admin.Verification;
using Eirene.BLL.Models.Core.Doctor.Verification;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Enumerators;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class DoctorVerificationService : IDoctorVerificationService
    {
        private readonly IDoctorVerificationRepository _verificationRepository;
        private readonly IDoctorDocumentRepository _documentRepository;
        private readonly IDoctorAuditLogRepository _auditLogRepository;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorVerificationService> _logger;

        public DoctorVerificationService(
            IDoctorVerificationRepository verificationRepository,
            IDoctorDocumentRepository documentRepository,
            IDoctorAuditLogRepository auditLogRepository,
            IDoctorProfileRepository doctorProfileRepository,
            IDocumentStorageService documentStorageService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DoctorVerificationService> logger)
        {
            _verificationRepository = verificationRepository;
            _documentRepository = documentRepository;
            _auditLogRepository = auditLogRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _documentStorageService = documentStorageService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<DoctorVerificationModel>> SubmitDoctorDocumentsAsync(string doctorId, SubmitDocumentsRequest request)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return Result.Failure<DoctorVerificationModel>("Doctor profile not found.");

                var verification = doctor.DoctorVerification;

                if (verification == null)
                {
                    verification = new DoctorVerification
                    {
                        DoctorId = doctorId,
                        LicenseNumber = request.LicenseNumber,
                        IssuingAuthority = request.IssuingAuthority,
                        LicenseExpiryDate = DateTime.SpecifyKind(request.LicenseExpiryDate, DateTimeKind.Utc),
                        SyndicateMembershipId = request.SyndicateMembershipId,
                        HospitalAffiliation = request.HospitalAffiliation,
                        VerificationStatus = VerificationStatus.Pending,
                        SubmittedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    await _verificationRepository.AddAsync(verification);
                }
                else
                {
                    verification.LicenseNumber = request.LicenseNumber;
                    verification.IssuingAuthority = request.IssuingAuthority;
                    verification.LicenseExpiryDate = DateTime.SpecifyKind(request.LicenseExpiryDate, DateTimeKind.Utc);
                    verification.SyndicateMembershipId = request.SyndicateMembershipId;
                    verification.HospitalAffiliation = request.HospitalAffiliation;
                    verification.VerificationStatus = VerificationStatus.Pending;
                    verification.LastUpdatedAt = DateTime.UtcNow;
                    await _verificationRepository.UpdateAsync(verification);
                }

                if (request.Files == null || request.Files.Count == 0)
                {
                    return Result.Failure<DoctorVerificationModel>("No documents were provided. Please ensure files are attached with the key 'Files'.");
                }

                for (int i = 0; i < request.Files.Count; i++)
                {
                    var file = request.Files[i];
                    var docType = request.DocumentTypes.ElementAtOrDefault(i);

                    var uploadResult = await _documentStorageService.UploadDocumentAsync(file, doctorId);
                    if (!uploadResult.IsSuccess)
                        return Result.Failure<DoctorVerificationModel>($"Failed to upload document: {uploadResult.Error}");

                    var doc = new DoctorDocument
                    {
                        DoctorId = doctorId,
                        DocumentType = docType,
                        FileName = file.FileName,
                        FilePath = uploadResult.Url!,
                        UploadedAt = DateTime.UtcNow,
                        ReviewStatus = DocumentReviewStatus.Pending
                    };
                    await _documentRepository.AddAsync(doc);
                }

                var auditLog = new DoctorAuditLog
                {
                    DoctorId = doctorId,
                    AdminId = doctorId, // Since doctor is doing this, we can leave AdminId as doctorId or system, but we need an ApplicationUser FK. Wait, AdminId is FK to ApplicationUser. So using DoctorId (which is UserId) works.
                    Action = "DocumentsSubmitted",
                    Reason = "Doctor submitted documents for verification",
                    Timestamp = DateTime.UtcNow
                };
                await _auditLogRepository.AddAsync(auditLog);

                await _unitOfWork.SaveChangesAsync();

                var model = _mapper.Map<DoctorVerificationModel>(verification);
                var docs = await _documentRepository.FindAsync(d => d.DoctorId == doctorId);
                model.Documents = _mapper.Map<List<DoctorDocumentModel>>(docs);
                return Result.Success(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting documents for doctor {DoctorId}", doctorId);
                return Result.Failure<DoctorVerificationModel>("An error occurred while submitting documents.");
            }
        }

        public async Task<Result<DoctorVerificationModel>> ReviewDoctorAsync(string adminId, string doctorId, ReviewDoctorRequest request)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return Result.Failure<DoctorVerificationModel>("Doctor profile not found.");

                var verification = doctor.DoctorVerification;

                if (verification == null)
                    return Result.Failure<DoctorVerificationModel>("Doctor verification record not found.");

                var currentStatus = verification.VerificationStatus;
                var newStatus = request.NewStatus;

                bool isValidTransition = false;
                if (currentStatus == VerificationStatus.Pending && newStatus == VerificationStatus.UnderReview) isValidTransition = true;
                else if (currentStatus == VerificationStatus.UnderReview && (newStatus == VerificationStatus.Approved || newStatus == VerificationStatus.Rejected)) isValidTransition = true;
                else if (currentStatus == VerificationStatus.Approved && newStatus == VerificationStatus.Suspended) isValidTransition = true;
                else if (currentStatus == VerificationStatus.Suspended && newStatus == VerificationStatus.Approved) isValidTransition = true;

                if (!isValidTransition)
                {
                    return Result.Failure<DoctorVerificationModel>($"Invalid state transition from {currentStatus} to {newStatus}.");
                }

                verification.VerificationStatus = newStatus;
                verification.LastUpdatedAt = DateTime.UtcNow;
                await _verificationRepository.UpdateAsync(verification);

                if (newStatus == VerificationStatus.Approved)
                {
                    doctor.IsVerified = true;
                }
                else if (newStatus == VerificationStatus.Rejected || newStatus == VerificationStatus.Suspended)
                {
                    doctor.IsVerified = false;
                }
                await _doctorProfileRepository.UpdateAsync(doctor);

                var auditLog = new DoctorAuditLog
                {
                    DoctorId = doctorId,
                    AdminId = adminId,
                    Action = $"StatusChangedTo{newStatus}",
                    Reason = request.Reason,
                    Timestamp = DateTime.UtcNow
                };
                await _auditLogRepository.AddAsync(auditLog);

                await _unitOfWork.SaveChangesAsync();

                var model = _mapper.Map<DoctorVerificationModel>(verification);
                var docs = await _documentRepository.FindAsync(d => d.DoctorId == doctorId);
                model.Documents = _mapper.Map<List<DoctorDocumentModel>>(docs);
                return Result.Success(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing doctor {DoctorId}", doctorId);
                return Result.Failure<DoctorVerificationModel>("An error occurred while reviewing doctor.");
            }
        }

        public async Task<Result<List<DoctorVerificationModel>>> GetPendingDoctorsAsync()
        {
            try
            {
                var verifications = await _verificationRepository.FindAsync(v => v.VerificationStatus == VerificationStatus.Pending || v.VerificationStatus == VerificationStatus.UnderReview);
                
                var models = new List<DoctorVerificationModel>();
                foreach (var verification in verifications)
                {
                    var model = _mapper.Map<DoctorVerificationModel>(verification);
                    var docs = await _documentRepository.FindAsync(d => d.DoctorId == verification.DoctorId);
                    model.Documents = _mapper.Map<List<DoctorDocumentModel>>(docs);
                    models.Add(model);
                }

                return Result.Success(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending doctors");
                return Result.Failure<List<DoctorVerificationModel>>("An error occurred while getting pending doctors.");
            }
        }

        public async Task<Result<List<DoctorAuditLogModel>>> GetDoctorAuditLogAsync(string doctorId)
        {
            try
            {
                var logs = await _auditLogRepository.FindAsync(a => a.DoctorId == doctorId);
                var sortedLogs = logs.OrderByDescending(l => l.Timestamp).ToList();
                var models = _mapper.Map<List<DoctorAuditLogModel>>(sortedLogs);
                return Result.Success(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log for doctor {DoctorId}", doctorId);
                return Result.Failure<List<DoctorAuditLogModel>>("An error occurred while getting audit logs.");
            }
        }

        public async Task<Result<bool>> DoctorUploadedDocuments(string doctorId)
        {
            try
            {
                var verifications = await _verificationRepository.FindAsync(v => v.DoctorId == doctorId);
                var verification = verifications.FirstOrDefault();
                if (verification is null)
                {
                    _logger.LogInformation("Doctor {DoctorId} has not uploaded any documents", doctorId);
                    return Result.Success(false);
                }

                _logger.LogInformation("Doctor {DoctorId} has uploaded documents", doctorId);
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if doctor {DoctorId} has uploaded documents", doctorId);
                return Result.Failure<bool>("An error occurred while checking if doctor has uploaded documents.");
            }
        }
    }
}
