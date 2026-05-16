using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IPatientProfileRepository _patientProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoleManagementService> _logger;
        private readonly IMapper _mapper;

        public RoleManagementService(
            UserManager<ApplicationUser> userManager,
            IDoctorProfileRepository doctorProfileRepository,
            IPatientProfileRepository patientProfileRepository,
            IUnitOfWork unitOfWork,
            ILogger<RoleManagementService> _logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _doctorProfileRepository = doctorProfileRepository;
            _patientProfileRepository = patientProfileRepository;
            _unitOfWork = unitOfWork;
            this._logger = _logger;
            _mapper = mapper;
        }

        public async Task<Result> AssignRoleAsync(string adminId, string userId, string role)
        {
            try
            {
                if (adminId == userId)
                {
                    _logger.LogWarning("Admin {AdminId} attempted to alter their own role.", adminId);
                    return Result.Failure("You cannot modify your own role.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to assign role to non-existent user {UserId}.", userId);
                    return Result.Failure("User not found.");
                }

                var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if (currentRole != null && currentRole == role)
                {
                    _logger.LogWarning("User {UserId} already has role '{Role}'.", userId, role);
                    return Result.Success();
                }

                if (currentRole != null)
                {
                    switch (currentRole)
                    {
                        case Roles.Doctor:
                            var doctorProfile = await _doctorProfileRepository.GetByIdAsync(userId);
                            if (doctorProfile != null)
                            {
                                await _doctorProfileRepository.DeleteAsync(doctorProfile);
                            }
                            break;
                        case Roles.Patient:
                            var patientProfile = await _patientProfileRepository.GetByIdAsync(userId);
                            if (patientProfile != null)
                            {
                                await _patientProfileRepository.DeleteAsync(patientProfile);
                            }
                            break;
                    }
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                }

                switch (role)
                {
                    case Roles.Doctor:
                        await _doctorProfileRepository.AddAsync(new DoctorProfile
                        {
                            Id = userId,
                            JoinedAt = DateTime.UtcNow
                        });
                        break;
                    case Roles.Patient:
                        await _patientProfileRepository.AddAsync(new PatientProfile
                        {
                            Id = userId,
                            ProfilePhotoUrl = $"https://api.dicebear.com/9.x/notionists/png?seed={user.Email}"
                        });
                        break;
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                await _unitOfWork.SaveChangesAsync();

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully assigned role '{Role}' to user {UserId} by admin {AdminId} and migrated profile.", role, userId, adminId);
                    return Result.Success();
                }
                else
                {
                    _logger.LogWarning("Failed to assign role '{Role}' to user {UserId}. Errors: {Errors}", role, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Result.Failure("Failed to assign role.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning role '{Role}' to user {UserId} by admin {AdminId}.", role, userId, adminId);
                return Result.Failure("An error occurred while assigning the role.");
            }
        }

        public async Task<Result<List<DoctorModel>>> GetPendingDoctorsAsync()
        {
            try
            {
                var pendingDoctors = await _doctorProfileRepository.FindAsync(d => !d.IsVerified);
                if (pendingDoctors == null)
                {
                    return Result.Failure<List<DoctorModel>>("No pending doctors found.");
                }

                var doctorModels = _mapper.Map<List<DoctorModel>>(pendingDoctors);
                return Result.Success(doctorModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving pending doctors.");
                return Result.Failure<List<DoctorModel>>("An error occurred while retrieving pending doctors.");
            }
        }

        public async Task<Result> ApproveDoctorAsync(string doctorId)
        {
            try
            {
                var doctor = await _doctorProfileRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return Result.Failure("Doctor profile not found.");
                }

                if (doctor.IsVerified)
                {
                    return Result.Failure("Doctor is already verified.");
                }

                doctor.Verify();

                await _doctorProfileRepository.UpdateAsync(doctor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor {DoctorId} has been verified.", doctorId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while approving doctor {DoctorId}.", doctorId);
                return Result.Failure("An error occurred while approving the doctor.");
            }
        }
    }
}
