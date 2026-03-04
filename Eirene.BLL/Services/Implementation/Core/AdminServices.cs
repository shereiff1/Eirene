using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Services.Abstraction.Core;
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
        private readonly IAdminProfileRepository _adminProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminServices> _logger;
        private readonly IMapper _mapper;

        public AdminServices(
            UserManager<ApplicationUser> userManager,
            ICommunityGroupRepository communityGroupRepository,
            IAdminProfileRepository adminProfileRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminServices> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _communityGroupRepository = communityGroupRepository;
            _adminProfileRepository = adminProfileRepository;
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
                if (currentRole != null) await _userManager.RemoveFromRoleAsync(user, currentRole);
                var result = await _userManager.AddToRoleAsync(user, role);
                await _unitOfWork.SaveChangesAsync();
                if (result.Succeeded)
                    _logger.LogInformation("Successfully assigned role '{Role}' to user {UserId} by admin {AdminId}.", role, userId, adminId);
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
                var group = await _communityGroupRepository.GetByIdWithDetailsAsync(groupId);
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

                group.Members ??= new List<ApplicationUser>();

                var isMember = group.Members.Any(m => m.Id == userId);

                if (assign && !isMember)
                {
                    group.Members.Add(user);
                    _logger.LogInformation("Added user {UserId} to group {GroupId}.", userId, groupId);
                }
                else if (!assign && isMember)
                {
                    group.Members.Remove(group.Members.First(m => m.Id == userId));
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
    }
}
