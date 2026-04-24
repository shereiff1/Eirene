using AutoMapper;
using Eirene.BLL.Enumerators;
using Eirene.BLL.Models.Community.Comment;
using Eirene.BLL.Services.Abstraction.Community;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Repository.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Eirene.BLL.Services.Implementation.Community
{
    public class CommunityCommentServices : ICommunityCommentServices
    {
        private readonly ILogger<CommunityCommentServices> _logger;
        private readonly IMapper _mapper;
        private readonly ICommunityCommentRepository _communityCommentRepository;
        private readonly ICommunityPostRepository _communityPostRepository;
        private readonly IUserCommunityGroupRepository _userCommunityGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommunityCommentServices(
            ILogger<CommunityCommentServices> logger,
            IMapper mapper,
            ICommunityCommentRepository communityCommentRepository,
            ICommunityPostRepository communityPostRepository,
            IUserCommunityGroupRepository userCommunityGroupRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _communityCommentRepository = communityCommentRepository;
            _communityPostRepository = communityPostRepository;
            _userCommunityGroupRepository = userCommunityGroupRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool IsSuccess, List<CommunityCommentDTO>? Comments)> GetByPostIdAsync(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to retrieve comments for post {PostId}", postId);
                    return (false, null);
                }

                var post = await _communityPostRepository.GetByIdAsync(postId);
                if (post == null || post.IsDeleted)
                {
                    _logger.LogWarning("Cannot retrieve comments: Post with ID {PostId} not found or deleted", postId);
                    return (false, null);
                }

                if (!await CanAccessGroupContentAsync(post.CommunityGroupId, userId))
                {
                    _logger.LogWarning("User {UserId} is not allowed to access comments for post {PostId}", userId, postId);
                    return (false, null);
                }

                var comments = await _communityCommentRepository.GetByPostIdWithDetailsAsync(postId);

                if (!comments.Any())
                {
                    _logger.LogInformation("No comments found for post with ID: {PostId}", postId);
                    return (true, new List<CommunityCommentDTO>());
                }

                var topLevelComments = comments.Where(c => c.ParentCommentId == null && !c.IsDeleted).ToList();
                var commentDTOs = _mapper.Map<List<CommunityCommentDTO>>(topLevelComments);

                _logger.LogInformation("Retrieved {Count} comments for post {PostId}", commentDTOs.Count, postId);
                return (true, commentDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, CommunityCommentDTO? Comment)> GetByIdAsync(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to retrieve comment {CommentId}", id);
                    return (false, null);
                }

                var comment = await _communityCommentRepository.GetByIdWithDetailsAsync(id);

                if (comment == null || comment.IsDeleted)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found or deleted", id);
                    return (false, null);
                }

                var post = await _communityPostRepository.GetByIdAsync(comment.PostId);
                if (post == null || post.IsDeleted || !await CanAccessGroupContentAsync(post.CommunityGroupId, userId))
                {
                    _logger.LogWarning("User {UserId} is not allowed to access comment {CommentId}", userId, id);
                    return (false, null);
                }

                var commentDTO = _mapper.Map<CommunityCommentDTO>(comment);
                return (true, commentDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment with ID {CommentId}", id);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<CommunityCommentDTO>? Replies)> GetRepliesByCommentIdAsync(
            Guid commentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized user tried to retrieve replies for comment {CommentId}", commentId);
                    return (false, null);
                }

                var parentComment = await _communityCommentRepository.GetByIdAsync(commentId);
                if (parentComment == null || parentComment.IsDeleted)
                {
                    _logger.LogWarning("Cannot retrieve replies: Comment with ID {CommentId} not found or deleted", commentId);
                    return (false, null);
                }

                var post = await _communityPostRepository.GetByIdAsync(parentComment.PostId);
                if (post == null || post.IsDeleted || !await CanAccessGroupContentAsync(post.CommunityGroupId, userId))
                {
                    _logger.LogWarning("User {UserId} is not allowed to access replies for comment {CommentId}", userId, commentId);
                    return (false, null);
                }

                var replies = await _communityCommentRepository.GetRepliesByCommentIdAsync(commentId);

                if (replies == null || !replies.Any())
                {
                    _logger.LogInformation("No replies found for comment with ID: {CommentId}", commentId);
                    return (true, new List<CommunityCommentDTO>());
                }

                var activeReplies = replies.Where(r => !r.IsDeleted).ToList();
                var replyDTOs = _mapper.Map<List<CommunityCommentDTO>>(activeReplies);

                _logger.LogInformation("Retrieved {Count} replies for comment {CommentId}", replyDTOs.Count, commentId);
                return (true, replyDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving replies for comment {CommentId}", commentId);
                return (false, null);
            }
        }


        public async Task<(bool IsSuccess, string Message, CommunityCommentDTO? CreatedComment)> CreateAsync(AddCommunityComment model)
        {
            try
            {
                var userId = _httpContextAccessor
                    ?.HttpContext
                    ?.User
                    ?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(model.Content))
                {
                    _logger.LogWarning("Invalid comment data: Content or UserId is empty");
                    return (false, "Comment content is required.", null);
                }

                var post = await _communityPostRepository.GetByIdAsync(model.PostId);
                if (post == null || post.IsDeleted)
                {
                    _logger.LogWarning("Cannot create comment: Post with ID {PostId} not found or deleted", model.PostId);
                    return (false, "Community post not found.", null);
                }

                var membershipValidationResult =
                    await ValidateGroupMessagingPermissionAsync(post.CommunityGroupId, userId);
                if (!membershipValidationResult.IsAllowed)
                {
                    _logger.LogWarning(
                        "User {UserId} is not allowed to comment in group {GroupId}. Reason: {Reason}",
                        userId,
                        post.CommunityGroupId,
                        membershipValidationResult.Message);

                    return (false, membershipValidationResult.Message, null);
                }

                if (model.ParentCommentId.HasValue)
                {
                    var parentComment = await _communityCommentRepository.GetByIdAsync(model.ParentCommentId.Value);
                    if (parentComment == null || parentComment.IsDeleted)
                    {
                        _logger.LogWarning(
                            "Cannot create reply: Parent comment with ID {ParentCommentId} not found or deleted",
                            model.ParentCommentId.Value);
                        return (false, "Parent comment not found.", null);
                    }

                    if (parentComment.PostId != model.PostId)
                    {
                        _logger.LogWarning("Parent comment {ParentCommentId} does not belong to post {PostId}",
                            model.ParentCommentId.Value, model.PostId);
                        return (false, "Parent comment does not belong to the selected post.", null);
                    }
                }

                var comment = _mapper.Map<CommunityComment>(model);
                comment.UserId = userId;

                var createdComment = await _communityCommentRepository.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                if (createdComment == null)
                {
                    _logger.LogWarning("Failed to create comment for post {PostId}", model.PostId);
                    return (false, "Failed to create the comment.", null);
                }

                post.CommentsCount++;
                await _communityPostRepository.UpdateAsync(post);

                if (model.ParentCommentId.HasValue)
                {
                    var parentComment = await _communityCommentRepository.GetByIdAsync(model.ParentCommentId.Value);
                    if (parentComment != null)
                    {
                        parentComment.RepliesCount++;
                        await _communityCommentRepository.UpdateAsync(parentComment);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var commentWithDetails =
                    await _communityCommentRepository.GetByIdWithDetailsAsync(createdComment.Id);

                var commentDTO = _mapper.Map<CommunityCommentDTO>(commentWithDetails);

                _logger.LogInformation("Comment created successfully with ID: {CommentId} by user {UserId}",
                    createdComment.Id, userId);

                return (true, "Comment created successfully.", commentDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId}", model.PostId);
                return (false, "An error occurred while creating the comment.", null);
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
                return (false, "You must join this community group before commenting.");
            }

            if (membership.IsBanned)
            {
                return (false, "You are banned from this community group and cannot comment.");
            }

            if (membership.HasActiveTimeout(DateTime.UtcNow))
            {
                return (false, "You are temporarily timed out in this community group and cannot comment yet.");
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

        public async Task<bool> UpdateAsync(EditCommunityComment model)
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
                    _logger.LogWarning("Unauthorized user tried to update comment {CommentId}", model.Id);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(model.Content))
                {
                    _logger.LogWarning("Invalid content for comment {CommentId}", model.Id);
                    return false;
                }

                var existingComment = await _communityCommentRepository.GetByIdAsync(model.Id);

                if (existingComment == null || existingComment.IsDeleted)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found or deleted", model.Id);
                    return false;
                }

                // Authorization check
                if (existingComment.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to edit comment {CommentId}", userId, model.Id);
                    return false;
                }

                existingComment.Content = model.Content;
                existingComment.UpdatedOn = DateTime.UtcNow;
                existingComment.IsEdited = true;

                var result = await _communityCommentRepository.UpdateAsync(existingComment);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Comment {CommentId} updated successfully by user {UserId}", model.Id, userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", model.Id);
                return false;
            }
        }


        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var comment = await _communityCommentRepository.GetByIdAsync(id);

                if (comment == null || comment.IsDeleted)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found or already deleted", id);
                    return false;
                }

                comment.IsDeleted = true;
                var result = await _communityCommentRepository.UpdateAsync(comment);

                if (result)
                {
                    var post = await _communityPostRepository.GetByIdAsync(comment.PostId);
                    if (post != null && post.CommentsCount > 0)
                    {
                        post.CommentsCount--;
                        await _communityPostRepository.UpdateAsync(post);
                    }

                    if (comment.ParentCommentId.HasValue)
                    {
                        var parentComment =
                            await _communityCommentRepository.GetByIdAsync(comment.ParentCommentId.Value);
                        if (parentComment != null && parentComment.RepliesCount > 0)
                        {
                            parentComment.RepliesCount--;
                            await _communityCommentRepository.UpdateAsync(parentComment);
                        }
                    }

                    _logger.LogInformation("Comment {CommentId} deleted successfully", id);
                }
                await _unitOfWork.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return false;
            }
        }
    }
}
