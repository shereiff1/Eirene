using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class AdminProfileService : IAdminProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminProfileRepository _adminProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminProfileService> _logger;
        private readonly IMapper _mapper;

        public AdminProfileService(
            UserManager<ApplicationUser> userManager,
            IAdminProfileRepository adminProfileRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminProfileService> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _adminProfileRepository = adminProfileRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<List<AdminModel>>> GetAllAsync()
        {
            try
            {
                var profiles = await _adminProfileRepository.GetAllAsync();
                if (profiles == null)
                {
                    _logger.LogError("No admin profiles found.");
                    return Result.Failure<List<AdminModel>>("No admin profiles found.");
                }

                var adminModels = _mapper.Map<List<AdminModel>>(profiles);
                _logger.LogInformation("Retrieved {Count} admin profiles.", adminModels.Count);
                return Result.Success(adminModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all admin profiles.");
                return Result.Failure<List<AdminModel>>("An error occurred while retrieving admin profiles.");
            }
        }

        public async Task<Result<AdminModel>> GetByIdAsync(string adminId)
        {
            try
            {
                var profile = await _adminProfileRepository.GetByIdAsync(adminId);
                if (profile == null)
                {
                    _logger.LogError("Admin profile with id {AdminId} not found.", adminId);
                    return Result.Failure<AdminModel>("Admin profile not found.");
                }

                var adminModel = _mapper.Map<AdminModel>(profile);
                _logger.LogInformation("Admin profile {AdminId} retrieved.", adminId);
                return Result.Success(adminModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving admin profile {AdminId}.", adminId);
                return Result.Failure<AdminModel>("An error occurred while retrieving the profile.");
            }
        }

        public async Task<Result<AdminModel>> CreateAdminProfileAsync(string userId)
        {
            try
            {
                var existingProfile = await _adminProfileRepository.GetByIdAsync(userId);
                if (existingProfile != null)
                    return Result.Failure<AdminModel>("Admin profile already exists for this user.");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Failed to create admin profile: User {UserId} not found.", userId);
                    return Result.Failure<AdminModel>("User not found.");
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
                return Result.Success(adminModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating admin profile for user {UserId}.", userId);
                return Result.Failure<AdminModel>("An error occurred while creating the profile.");
            }
        }
    }
}
