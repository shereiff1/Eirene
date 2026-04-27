using AutoMapper;
using Eirene.BLL.Models.Community.Post;
using Eirene.BLL.Services.Abstraction.Community;
using Eirene.BLL.Enumerators;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Community;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Community
{
    public class CommunityPostServices : ICommunityPostServices
    {
        private readonly ILogger<CommunityPostServices> _logger;
        private readonly IMapper _mapper;
        private readonly ICommunityPostRepository _communityPostRepository;
        private readonly ICommunityGroupRepository _communityGroupRepository;
        private readonly IUserCommunityGroupRepository _userCommunityGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommunityPostServices(
            ILogger<CommunityPostServices> logger,
            IMapper mapper,
            ICommunityPostRepository communityPostRepository,
            ICommunityGroupRepository communityGroupRepository,
            IUserCommunityGroupRepository userCommunityGroupRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _communityPostRepository = communityPostRepository;
            _communityGroupRepository = communityGroupRepository;
            _userCommunityGroupRepository = userCommunityGroupRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetAllAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to retrieve all community posts");
                    return (false, null);
                }

                var posts = await _communityPostRepository.GetAllWithDetailsAsync();

                if (posts == null || !posts.Any())
                {
                    _logger.LogInformation("No community posts found");
                    return (true, new List<CommunityPostDTO>());
                }

                var activePosts = new List<CommunityPost>();
                foreach (var post in posts.Where(p => !p.IsDeleted))
                {
                    if (await CanAccessGroupContentAsync(post.CommunityGroupId, userId))
                    {
                        activePosts.Add(post);
                    }
                }

                var postDTOs = _mapper.Map<List<CommunityPostDTO>>(activePosts);

                if (!IsPrivilegedUser())
                    postDTOs.ForEach(SanitizePostPersonalData);

                _logger.LogInformation("Retrieved {Count} community posts", postDTOs.Count);
                return (true, postDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all community posts");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByGroupIdAsync(Guid groupId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to retrieve posts for group {GroupId}", groupId);
                    return (false, null);
                }

                if (!await CanAccessGroupContentAsync(groupId, userId))
                {
                    _logger.LogWarning("User {UserId} is not allowed to access posts for group {GroupId}", userId, groupId);
                    return (false, null);
                }

                var posts = await _communityPostRepository.GetByGroupIdWithDetailsAsync(groupId);

                if (posts == null || !posts.Any())
                {
                    _logger.LogInformation("No posts found for group {GroupId}", groupId);
                    return (true, new List<CommunityPostDTO>());
                }

                var activePosts = posts.Where(p => !p.IsDeleted).ToList();
                var postDTOs = _mapper.Map<List<CommunityPostDTO>>(activePosts);

                if (!IsPrivilegedUser())
                    postDTOs.ForEach(SanitizePostPersonalData);

                _logger.LogInformation("Retrieved {Count} posts for group {GroupId}", postDTOs.Count, groupId);
                return (true, postDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for group {GroupId}", groupId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, CommunityPostDTO? Post)> GetByIdAsync(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to retrieve post {PostId}", id);
                    return (false, null);
                }

                var post = await _communityPostRepository.GetByIdWithDetailsAsync(id);

                if (post == null || post.IsDeleted)
                {
                    _logger.LogWarning("Post with ID {PostId} not found or deleted", id);
                    return (false, null);
                }

                if (!await CanAccessGroupContentAsync(post.CommunityGroupId, userId))
                {
                    _logger.LogWarning("User {UserId} is not allowed to access post {PostId}", userId, id);
                    return (false, null);
                }

                var postDTO = _mapper.Map<CommunityPostDTO>(post);

                if (!IsPrivilegedUser())
                    SanitizePostPersonalData(postDTO);

                return (true, postDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post with ID {PostId}", id);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByUserIdAsync(string userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("UserId is null or empty");
                    return (false, null);
                }

                var posts = await _communityPostRepository.GetByUserIdWithDetailsAsync(userId);

                if (posts == null || !posts.Any())
                {
                    _logger.LogInformation("No posts found for user {UserId}", userId);
                    return (true, new List<CommunityPostDTO>());
                }

                var activePosts = new List<CommunityPost>();
                foreach (var post in posts.Where(p => !p.IsDeleted))
                {
                    if (await CanAccessGroupContentAsync(post.CommunityGroupId, currentUserId))
                    {
                        activePosts.Add(post);
                    }
                }

                var postDTOs = _mapper.Map<List<CommunityPostDTO>>(activePosts);

                if (!IsPrivilegedUser())
                    postDTOs.ForEach(SanitizePostPersonalData);

                _logger.LogInformation("Retrieved {Count} posts for user {UserId}", postDTOs.Count, userId);
                return (true, postDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", userId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, string Message, CommunityPostDTO? CreatedPost)> CreateAsync(AddCommunityPost model)
        {
            try
            {
                var userId = _httpContextAccessor
                    ?.HttpContext
                    ?.User
                    ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value;

                if (string.IsNullOrWhiteSpace(model.Content) || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid post data: Content or UserId is empty");
                    return (false, "Post content is required.", null);
                }

                var group = await _communityGroupRepository.GetByIdAsync(model.CommunityGroupId);
                if (group == null)
                {
                    _logger.LogWarning("Cannot create post: Group with ID {GroupId} not found", model.CommunityGroupId);
                    return (false, "Community group not found.", null);
                }

                var membershipValidationResult =
                    await ValidateGroupMessagingPermissionAsync(model.CommunityGroupId, userId);
                if (!membershipValidationResult.IsAllowed)
                {
                    _logger.LogWarning(
                        "User {UserId} is not allowed to create a post in group {GroupId}. Reason: {Reason}",
                        userId,
                        model.CommunityGroupId,
                        membershipValidationResult.Message);

                    return (false, membershipValidationResult.Message, null);
                }

                var post = _mapper.Map<CommunityPost>(model);
                post.UserId = userId;

                var createdPost = await _communityPostRepository.AddAsync(post);
                await _unitOfWork.SaveChangesAsync();

                if (createdPost == null)
                {
                    _logger.LogWarning("Failed to create post in group {GroupId}", model.CommunityGroupId);
                    return (false, "Failed to create the post.", null);
                }

                var postWithDetails = await _communityPostRepository.GetByIdWithDetailsAsync(createdPost.Id);
                var postDTO = _mapper.Map<CommunityPostDTO>(postWithDetails);

                if (!IsPrivilegedUser())
                    SanitizePostPersonalData(postDTO);

                _logger.LogInformation("Post created successfully with ID: {PostId} by user {UserId} in group {GroupId}",
                    createdPost.Id, userId, model.CommunityGroupId);

                return (true, "Post created successfully.", postDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post in group {GroupId}", model.CommunityGroupId);
                return (false, "An error occurred while creating the post.", null);
            }
        }

        private async Task<(bool IsAllowed, string Message)> ValidateGroupMessagingPermissionAsync(Guid groupId, string userId)
        {
            if (IsCurrentUserAdmin())
            {
                return (true, string.Empty);
            }

            var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
            if (membership == null)
            {
                return (false, "You must join this community group before posting.");
            }

            if (membership.IsBanned)
            {
                return (false, "You are banned from this community group and cannot post.");
            }

            if (membership.HasActiveTimeout(DateTime.UtcNow))
            {
                return (false, "You are temporarily timed out in this community group and cannot post yet.");
            }

            return (true, string.Empty);
        }

        private async Task<bool> CanAccessGroupContentAsync(Guid groupId, string userId)
        {
            if (IsCurrentUserAdmin())
            {
                return true;
            }

            var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
            return membership != null && !membership.IsBanned;
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor
                ?.HttpContext
                ?.User
                ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value;
        }

        private bool IsCurrentUserAdmin()
        {
            return _httpContextAccessor?.HttpContext?.User?.IsInRole(Roles.Admin) == true;
        }

        private bool IsPrivilegedUser()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.IsInRole(Roles.Admin) == true || user?.IsInRole(Roles.Doctor) == true;
        }

        private static void SanitizePostPersonalData(CommunityPostDTO post)
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


        public async Task<bool> UpdateAsync(EditCommunityPost model)
        {
            try
            {
                var userId = _httpContextAccessor
                    ?.HttpContext
                    ?.User
                    ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to update post {PostId}", model.Id);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.Content))
                {
                    _logger.LogWarning("Invalid content for post {PostId}", model.Id);
                    return false;
                }

                var existingPost = await _communityPostRepository.GetByIdAsync(model.Id);

                if (existingPost == null || existingPost.IsDeleted)
                {
                    _logger.LogWarning("Post with ID {PostId} not found or deleted", model.Id);
                    return false;
                }

                if (existingPost.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to edit post {PostId}", userId, model.Id);
                    return false;
                }

                existingPost.Content = model.Content;
                existingPost.UpdatedOn = DateTime.UtcNow;
                existingPost.IsEdited = true;

                var result = await _communityPostRepository.UpdateAsync(existingPost);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Post {PostId} updated successfully by user {UserId}", model.Id, userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", model.Id);
                return false;
            }
        }


        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var post = await _communityPostRepository.GetByIdAsync(id);

                if (post == null || post.IsDeleted)
                {
                    _logger.LogWarning("Post with ID {PostId} not found or already deleted", id);
                    return false;
                }

                post.IsDeleted = true;
                var result = await _communityPostRepository.UpdateAsync(post);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Post {PostId} deleted successfully", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return false;
            }
        }
    }
}
