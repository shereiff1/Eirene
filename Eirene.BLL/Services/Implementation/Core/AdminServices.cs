using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Repository.Abstraction.Core;
using Eirene.DAL.Repository.Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class AdminServices : IAdminServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommunityGroupRepository _communityGroupRepository;
        private readonly IUserCommunityGroupRepository _userCommunityGroupRepository;
        private readonly IAdminProfileRepository _adminProfileRepository;
        private readonly IDoctorProfileRepository _doctorProfileRepository;
        private readonly IPatientProfileRepository _patientProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminServices> _logger;
        private readonly IMapper _mapper;

        public AdminServices(
            UserManager<ApplicationUser> userManager,
            ICommunityGroupRepository communityGroupRepository,
            IUserCommunityGroupRepository userCommunityGroupRepository,
            IAdminProfileRepository adminProfileRepository,
            IDoctorProfileRepository doctorProfileRepository,
            IPatientProfileRepository patientProfileRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminServices> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _communityGroupRepository = communityGroupRepository;
            _userCommunityGroupRepository = userCommunityGroupRepository;
            _adminProfileRepository = adminProfileRepository;
            _doctorProfileRepository = doctorProfileRepository;
            _patientProfileRepository = patientProfileRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<(bool IsSuccess, List<AdminModel>? Admins)> GetAllAsync()
        {
            try
            {
                var profiles = await _adminProfileRepository.GetAllAsync();
                if (profiles == null)
                {
                    _logger.LogError("No admin profiles found.");
                    return (false, null);
                }

                var adminModels = _mapper.Map<List<AdminModel>>(profiles);
                _logger.LogInformation("Retrieved {Count} admin profiles.", adminModels.Count);
                return (true, adminModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all admin profiles.");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, AdminModel? Admin)> GetByIdAsync(string adminId)
        {
            try
            {
                var profile = await _adminProfileRepository.GetByIdAsync(adminId);
                if (profile == null)
                {
                    _logger.LogError("Admin profile with id {AdminId} not found.", adminId);
                    return (false, null);
                }

                var adminModel = _mapper.Map<AdminModel>(profile);
                _logger.LogInformation("Admin profile {AdminId} retrieved.", adminId);
                return (true, adminModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving admin profile {AdminId}.", adminId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, string? Error, AdminModel? Admin)> CreateAdminProfileAsync(string userId)
        {
            try
            {
                var existingProfile = await _adminProfileRepository.GetByIdAsync(userId);
                if (existingProfile != null)
                    return (false, "Admin profile already exists for this user.", null);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Failed to create admin profile: User {UserId} not found.", userId);
                    return (false, "User not found.", null);
                }

                var newProfile = new AdminProfile
                {
                    Id = userId,
                    LastLogin = DateTime.UtcNow
                };

                await _adminProfileRepository.AddAsync(newProfile);
                await _unitOfWork.SaveChangesAsync();

                var createdProfile = await _adminProfileRepository.GetByIdAsync(userId);
                var adminModel = _mapper.Map<AdminModel>(createdProfile);

                _logger.LogInformation("Created admin profile for user {UserId}.", userId);
                return (true, null, adminModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating admin profile for user {UserId}.", userId);
                return (false, "An error occurred while creating the profile.", null);
            }
        }

        public async Task<bool> AssignRoleAsync(string adminId, string userId, string role)
        {
            try
            {
                if (adminId == userId)
                {
                    _logger.LogWarning("Admin {AdminId} attempted to alter their own role.", adminId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to assign role to non-existent user {UserId}.", userId);
                    return false;
                }

                var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if (currentRole != null && currentRole == role)
                {
                    _logger.LogWarning("User {UserId} already has role '{Role}'.", userId, role);
                    return true;
                }

                string? commonProfilePhotoUrl = null;
                string? phoneNumber = null;

                if (currentRole != null)
                {
                    switch (currentRole)
                    {
                        case Roles.Doctor:
                            var doctorProfile = await _doctorProfileRepository.GetByIdAsync(userId);
                            if (doctorProfile != null)
                            {
                                commonProfilePhotoUrl = doctorProfile.ProfilePhotoUrl;
                                phoneNumber = doctorProfile.PhoneNumber;
                                await _doctorProfileRepository.DeleteAsync(doctorProfile);
                            }
                            break;
                        case Roles.Patient:
                            var patientProfile = await _patientProfileRepository.GetByIdAsync(userId);
                            if (patientProfile != null)
                            {
                                commonProfilePhotoUrl = patientProfile.ProfilePhotoUrl;
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
                            ProfilePhotoUrl = commonProfilePhotoUrl,
                            PhoneNumber = phoneNumber ?? string.Empty,
                            JoinedAt = DateTime.UtcNow
                        });
                        break;
                    case Roles.Patient:
                        await _patientProfileRepository.AddAsync(new PatientProfile
                        {
                            Id = userId,
                            ProfilePhotoUrl = commonProfilePhotoUrl
                        });
                        break;
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                await _unitOfWork.SaveChangesAsync();

                if (result.Succeeded)
                    _logger.LogInformation("Successfully assigned role '{Role}' to user {UserId} by admin {AdminId} and migrated profile.", role, userId, adminId);
                else
                    _logger.LogWarning("Failed to assign role '{Role}' to user {UserId}. Errors: {Errors}", role, userId, string.Join(", ", result.Errors.Select(e => e.Description)));

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning role '{Role}' to user {UserId} by admin {AdminId}.", role, userId, adminId);
                return false;
            }
        }

        public async Task<bool> ManageCommunityGroupMembershipAsync(Guid groupId, string userId, bool assign)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("Attempted to manage membership for non-existent group {GroupId}.", groupId);
                    return false;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to manage membership for non-existent user {UserId}.", userId);
                    return false;
                }

                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);

                if (assign && membership == null)
                {
                    await _userCommunityGroupRepository.AddAsync(new UserCommunityGroup
                    {
                        CommunityGroupId = groupId,
                        UserId = userId
                    });

                    group.MemberCount++;
                    _logger.LogInformation("Added user {UserId} to group {GroupId}.", userId, groupId);
                }
                else if (!assign && membership != null)
                {
                    await _userCommunityGroupRepository.DeleteAsync(membership);
                    if (group.MemberCount > 0)
                    {
                        group.MemberCount--;
                    }

                    _logger.LogInformation("Removed user {UserId} from group {GroupId}.", userId, groupId);
                }
                else
                {
                    return true;
                }

                await _communityGroupRepository.UpdateAsync(group);
                var unitResult = await _unitOfWork.SaveChangesAsync();
                return unitResult > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while managing membership for user {UserId} in group {GroupId}. Assign: {Assign}", userId, groupId, assign);
                return false;
            }
        }

        public async Task<(bool IsSuccess, string Message)> BanUserFromGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot ban user {UserId} from group {GroupId}: membership not found.", userId, groupId);
                    return (false, "User is not a member of this community group.");
                }

                if (membership.IsBanned)
                {
                    return (false, "User is already banned from this community group.");
                }

                membership.IsBanned = true;
                membership.TimeoutUntil = null;

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} was banned from group {GroupId}.", userId, groupId);
                return (true, "User was banned from the community group successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while banning user {UserId} from group {GroupId}.", userId, groupId);
                return (false, "An error occurred while banning the user from the community group.");
            }
        }

        public async Task<(bool IsSuccess, string Message)> UnbanUserFromGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot unban user {UserId} from group {GroupId}: membership not found.", userId, groupId);
                    return (false, "User is not a member of this community group.");
                }

                if (!membership.IsBanned)
                {
                    return (false, "User is not banned from this community group.");
                }

                membership.IsBanned = false;

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} was unbanned from group {GroupId}.", userId, groupId);
                return (true, "User was unbanned from the community group successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while unbanning user {UserId} from group {GroupId}.", userId, groupId);
                return (false, "An error occurred while removing the ban from the user.");
            }
        }

        public async Task<(bool IsSuccess, string Message)> TimeoutUserInGroupAsync(Guid groupId, string userId, DateTime timeoutUntil)
        {
            try
            {
                if (timeoutUntil <= DateTime.UtcNow)
                {
                    return (false, "Timeout end date must be in the future.");
                }

                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot timeout user {UserId} in group {GroupId}: membership not found.", userId, groupId);
                    return (false, "User is not a member of this community group.");
                }

                membership.TimeoutUntil = timeoutUntil.ToUniversalTime();

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} was timed out in group {GroupId} until {TimeoutUntil}.", userId, groupId, membership.TimeoutUntil);
                return (true, "User timeout was applied successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying a timeout to user {UserId} in group {GroupId}.", userId, groupId);
                return (false, "An error occurred while applying the timeout.");
            }
        }

        public async Task<(bool IsSuccess, string Message)> RemoveTimeoutUserInGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot remove timeout for user {UserId} in group {GroupId}: membership not found.", userId, groupId);
                    return (false, "User is not a member of this community group.");
                }

                if (!membership.TimeoutUntil.HasValue)
                {
                    return (false, "User does not have an active timeout in this community group.");
                }

                membership.TimeoutUntil = null;

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Timeout removed for user {UserId} in group {GroupId}.", userId, groupId);
                return (true, "User timeout was removed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing timeout for user {UserId} in group {GroupId}.", userId, groupId);
                return (false, "An error occurred while removing the timeout.");
            }
        }
    }
}
