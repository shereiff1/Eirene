using AutoMapper;
using Eirene.BLL.Models.Community.Group;
using Eirene.BLL.Services.Abstraction.Community;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Community;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Community
{
    public class CommunityGroupServices : ICommunityGroupServices
    {
        private readonly ILogger<CommunityGroupServices> _logger;
        private readonly IMapper _mapper;
        private readonly ICommunityGroupRepository _communityGroupRepository;
        private readonly IUserCommunityGroupRepository _userCommunityGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommunityGroupServices(
            ILogger<CommunityGroupServices> logger,
            IMapper mapper,
            ICommunityGroupRepository communityGroupRepository,
            IUserCommunityGroupRepository userCommunityGroupRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _communityGroupRepository = communityGroupRepository;
            _userCommunityGroupRepository = userCommunityGroupRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool IsSuccess, List<CommunityGroupDTO>? Groups)> GetAllAsync()
        {
            try
            {
                var groups = await _communityGroupRepository.GetAllWithDetailsAsync();

                if (groups == null || !groups.Any())
                {
                    _logger.LogInformation("No community groups found");
                    return (true, new List<CommunityGroupDTO>());
                }

                var groupDTOs = _mapper.Map<List<CommunityGroupDTO>>(groups);

                if (!IsPrivilegedUser())
                    groupDTOs.ForEach(SanitizeGroupPersonalData);

                _logger.LogInformation("Retrieved {Count} community groups", groupDTOs.Count);
                return (true, groupDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all community groups");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, CommunityGroupDTO? Group)> GetByIdAsync(Guid id)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdWithDetailsAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("Community group with ID {GroupId} not found", id);
                    return (false, null);
                }

                var groupDTO = _mapper.Map<CommunityGroupDTO>(group);

                if (!IsPrivilegedUser())
                    SanitizeGroupPersonalData(groupDTO);

                return (true, groupDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving community group with ID {GroupId}", id);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<CommunityGroupDTO>? Groups)> GetByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId is null or empty");
                    return (false, null);
                }

                var groups = await _communityGroupRepository.GetByUserIdAsync(userId);

                if (groups == null || !groups.Any())
                {
                    _logger.LogInformation("No community groups found for user {UserId}", userId);
                    return (true, new List<CommunityGroupDTO>());
                }

                var groupDTOs = _mapper.Map<List<CommunityGroupDTO>>(groups);

                if (!IsPrivilegedUser())
                    groupDTOs.ForEach(SanitizeGroupPersonalData);

                _logger.LogInformation("Retrieved {Count} community groups for user {UserId}", groupDTOs.Count, userId);
                return (true, groupDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving community groups for user {UserId}", userId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, CommunityGroupDTO? CreatedGroup)> CreateAsync(AddCommunityGroup model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.CreatedByUserId))
                {
                    _logger.LogWarning("Invalid community group data: Name or CreatedByUserId is empty");
                    return (false, null);
                }

                var existingGroup = await _communityGroupRepository.GetByNameAsync(model.Name);
                if (existingGroup != null)
                {
                    _logger.LogWarning("Community group with name '{GroupName}' already exists", model.Name);
                    return (false, null);
                }

                var group = _mapper.Map<CommunityGroup>(model);
                var createdGroup = await _communityGroupRepository.AddAsync(group);
                await _unitOfWork.SaveChangesAsync();

                if (createdGroup != null)
                {
                    var groupWithDetails = await _communityGroupRepository.GetByIdWithDetailsAsync(createdGroup.Id);
                    var groupDTO = _mapper.Map<CommunityGroupDTO>(groupWithDetails);

                    if (!IsPrivilegedUser())
                        SanitizeGroupPersonalData(groupDTO);

                    _logger.LogInformation(
                        "Community group '{GroupName}' created successfully with ID: {GroupId} by user {UserId}",
                        model.Name, createdGroup.Id, model.CreatedByUserId);
                    return (true, groupDTO);
                }

                _logger.LogWarning("Failed to create community group '{GroupName}'", model.Name);
                return (false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community group '{GroupName}'", model.Name);
                return (false, null);
            }
        }

        public async Task<bool> UpdateAsync(EditCommunityGroup model)
        {
            try
            {
                var existingGroup = await _communityGroupRepository.GetByIdAsync(model.Id);

                if (existingGroup == null)
                {
                    _logger.LogWarning("Community group with ID {GroupId} not found", model.Id);
                    return false;
                }

                var groupWithSameName = await _communityGroupRepository.GetByNameAsync(model.Name);
                if (groupWithSameName != null && groupWithSameName.Id != model.Id)
                {
                    _logger.LogWarning("Another community group with name '{GroupName}' already exists", model.Name);
                    return false;
                }

                existingGroup.Name = model.Name;
                existingGroup.Description = model.Description;

                var result = await _communityGroupRepository.UpdateAsync(existingGroup);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Community group {GroupId} updated successfully", model.Id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating community group {GroupId}", model.Id);
                return false;
            }
        }

        public async Task<(bool IsSuccess, CommunityGroupWithDetails? Group)> GetByIdWithFullDetailsAsync(Guid id)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdWithDetailsAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("Community group with ID {GroupId} not found", id);
                    return (false, null);
                }

                var groupWithDetails = _mapper.Map<CommunityGroupWithDetails>(group);

                if (!IsPrivilegedUser())
                    SanitizeGroupWithDetailsPersonalData(groupWithDetails);

                return (true, groupWithDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving community group details with ID {GroupId}", id);
                return (false, null);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("Community group with ID {GroupId} not found", id);
                    return false;
                }

                var groupWithPosts = await _communityGroupRepository.GetByIdWithDetailsAsync(id);
                if (groupWithPosts?.Posts != null && groupWithPosts.Posts.Any(p => !p.IsDeleted))
                {
                    _logger.LogWarning("Cannot delete community group {GroupId}: It contains active posts", id);
                    return false;
                }

                var result = await _communityGroupRepository.DeleteAsync(group);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Community group {GroupId} deleted successfully", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting community group {GroupId}", id);
                return false;
            }
        }

        public async Task<(bool IsSuccess, string Message)> JoinGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("Community group {GroupId} not found", groupId);
                    return (false, "Community group not found.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return (false, "User not found.");
                }

                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership != null)
                {
                    _logger.LogInformation("User {UserId} is already a member of group {GroupId}", userId, groupId);
                    return (false, "You are already a member of this group.");
                }

                await _userCommunityGroupRepository.AddAsync(new UserCommunityGroup
                {
                    CommunityGroupId = groupId,
                    UserId = userId
                });

                group.MemberCount++;
                await _communityGroupRepository.UpdateAsync(group);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} joined group {GroupId}", userId, groupId);
                return (true, "Successfully joined the group.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group {GroupId} for user {UserId}", groupId, userId);
                return (false, "An error occurred while joining the group.");
            }
        }

        public async Task<(bool IsSuccess, string Message)> LeaveGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("Community group {GroupId} not found", groupId);
                    return (false, "Community group not found.");
                }

                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogInformation("User {UserId} is not a member of group {GroupId}", userId, groupId);
                    return (false, "You are not a member of this group.");
                }

                await _userCommunityGroupRepository.DeleteAsync(membership);
                if (group.MemberCount > 0)
                {
                    group.MemberCount--;
                }

                await _communityGroupRepository.UpdateAsync(group);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} left group {GroupId}", userId, groupId);
                return (true, "Successfully left the group.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group {GroupId} for user {UserId}", groupId, userId);
                return (false, "An error occurred while leaving the group.");
            }
        }

        public async Task<(bool IsSuccess, List<CommunityGroupDTO>? Groups)> GetJoinedByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId is null or empty");
                    return (false, null);
                }

                var groups = await _communityGroupRepository.GetJoinedGroupsByUserIdAsync(userId);

                if (groups == null || !groups.Any())
                {
                    _logger.LogInformation("No joined community groups found for user {UserId}", userId);
                    return (true, new List<CommunityGroupDTO>());
                }

                var groupDTOs = _mapper.Map<List<CommunityGroupDTO>>(groups);

                if (!IsPrivilegedUser())
                    groupDTOs.ForEach(SanitizeGroupPersonalData);

                _logger.LogInformation("Retrieved {Count} joined community groups for user {UserId}", groupDTOs.Count, userId);
                return (true, groupDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving joined community groups for user {UserId}", userId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<CommunityGroupDTO>? Groups)> GetUnjoinedByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId is null or empty");
                    return (false, null);
                }

                var groups = await _communityGroupRepository.GetUnjoinedGroupsByUserIdAsync(userId);

                if (groups == null || !groups.Any())
                {
                    _logger.LogInformation("No available community groups found for user {UserId}", userId);
                    return (true, new List<CommunityGroupDTO>());
                }

                var groupDTOs = _mapper.Map<List<CommunityGroupDTO>>(groups);

                if (!IsPrivilegedUser())
                    groupDTOs.ForEach(SanitizeGroupPersonalData);

                _logger.LogInformation("Retrieved {Count} available community groups for user {UserId}", groupDTOs.Count, userId);
                return (true, groupDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available community groups for user {UserId}", userId);
                return (false, null);
            }
        }

        private bool IsPrivilegedUser()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.IsInRole(Enumerators.Roles.Admin) == true || user?.IsInRole(Enumerators.Roles.Doctor) == true;
        }

        private static void SanitizeGroupPersonalData(CommunityGroupDTO group)
        {
            group.CreatedByUserId = string.Empty;
            group.CreatedByUserName = string.Empty;
        }

        private static void SanitizeGroupWithDetailsPersonalData(CommunityGroupWithDetails group)
        {
            if (group.Members != null)
            {
                foreach (var member in group.Members)
                {
                    member.Id = string.Empty;
                    member.UserName = string.Empty;
                    member.Email = string.Empty;
                }
            }

            if (group.Posts != null)
            {
                foreach (var post in group.Posts)
                {
                    post.UserId = string.Empty;
                    if (post.Comments != null)
                    {
                        foreach (var comment in post.Comments)
                        {
                            SanitizeCommentPersonalData(comment);
                        }
                    }
                }
            }
        }

        private static void SanitizeCommentPersonalData(Models.Community.Comment.CommunityCommentDTO comment)
        {
            comment.UserName = string.Empty;
            if (comment.Replies != null)
            {
                foreach (var reply in comment.Replies)
                {
                    SanitizeCommentPersonalData(reply);
                }
            }
        }
    }
}
