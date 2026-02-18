using AutoMapper;
using BLL.Models.Community.Post;
using BLL.Services.Abstraction.Community;
using DAL.Entities.Community;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Community;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Community
{
    public class CommunityPostServices : ICommunityPostServices
    {
        private readonly ILogger<CommunityPostServices> _logger;
        private readonly IMapper _mapper;
        private readonly ICommunityPostRepository _communityPostRepository;
        private readonly ICommunityGroupRepository _communityGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommunityPostServices(
            ILogger<CommunityPostServices> logger,
            IMapper mapper,
            ICommunityPostRepository communityPostRepository,
            ICommunityGroupRepository communityGroupRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _communityPostRepository = communityPostRepository;
            _communityGroupRepository = communityGroupRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetAllAsync()
        {
            try
            {
                var posts = await _communityPostRepository.GetAllWithDetailsAsync();

                if (posts == null || !posts.Any())
                {
                    _logger.LogInformation("No community posts found");
                    return (true, new List<CommunityPostDTO>());
                }

                var activePosts = posts.Where(p => !p.IsDeleted).ToList();
                var postDTOs = _mapper.Map<List<CommunityPostDTO>>(activePosts);

                _logger.LogInformation("Retrieved {Count} community posts", postDTOs.Count);
                return (true, postDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all community posts");
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByGroupIdAsync(int groupId)
        {
            try
            {
                var posts = await _communityPostRepository.GetByGroupIdWithDetailsAsync(groupId);

                if (posts == null || !posts.Any())
                {
                    _logger.LogInformation("No posts found for group {GroupId}", groupId);
                    return (true, new List<CommunityPostDTO>());
                }

                var activePosts = posts.Where(p => !p.IsDeleted).ToList();
                var postDTOs = _mapper.Map<List<CommunityPostDTO>>(activePosts);

                _logger.LogInformation("Retrieved {Count} posts for group {GroupId}", postDTOs.Count, groupId);
                return (true, postDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for group {GroupId}", groupId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, CommunityPostDTO? Post)> GetByIdAsync(int id)
        {
            try
            {
                var post = await _communityPostRepository.GetByIdWithDetailsAsync(id);

                if (post == null || post.IsDeleted)
                {
                    _logger.LogWarning("Post with ID {PostId} not found or deleted", id);
                    return (false, null);
                }

                var postDTO = _mapper.Map<CommunityPostDTO>(post);
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
                if (string.IsNullOrEmpty(userId))
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

                var activePosts = posts.Where(p => !p.IsDeleted).ToList();
                var postDTOs = _mapper.Map<List<CommunityPostDTO>>(activePosts);

                _logger.LogInformation("Retrieved {Count} posts for user {UserId}", postDTOs.Count, userId);
                return (true, postDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", userId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, CommunityPostDTO? CreatedPost)> CreateAsync(AddCommunityPost model)
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
                    return (false, null);
                }

                var group = await _communityGroupRepository.GetByIdAsync(model.CommunityGroupId);
                if (group == null)
                {
                    _logger.LogWarning("Cannot create post: Group with ID {GroupId} not found", model.CommunityGroupId);
                    return (false, null);
                }

                var post = _mapper.Map<CommunityPost>(model);
                post.UserId = userId;

                var createdPost = await _communityPostRepository.AddAsync(post);
                await _unitOfWork.SaveChangesAsync();

                if (createdPost == null)
                {
                    _logger.LogWarning("Failed to create post in group {GroupId}", model.CommunityGroupId);
                    return (false, null);
                }

                var postWithDetails = await _communityPostRepository.GetByIdWithDetailsAsync(createdPost.Id);
                var postDTO = _mapper.Map<CommunityPostDTO>(postWithDetails);

                _logger.LogInformation("Post created successfully with ID: {PostId} by user {UserId} in group {GroupId}",
                    createdPost.Id, userId, model.CommunityGroupId);

                return (true, postDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post in group {GroupId}", model.CommunityGroupId);
                return (false, null);
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


        public async Task<bool> DeleteAsync(int id)
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
