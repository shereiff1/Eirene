using AutoMapper;
using BLL.Models.Community.Comment;
using BLL.Services.Abstraction.Community;
using DAL.Entities.Community;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Community
{
    public class CommunityCommentServices : ICommunityCommentServices
    {
        private readonly ILogger<CommunityCommentServices> _logger;
        private readonly IMapper _mapper;
        private readonly ICommunityCommentRepository _communityCommentRepository;
        private readonly ICommunityPostRepository _communityPostRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CommunityCommentServices(
            ILogger<CommunityCommentServices> logger,
            IMapper mapper,
            ICommunityCommentRepository communityCommentRepository,
            ICommunityPostRepository communityPostRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _communityCommentRepository = communityCommentRepository;
            _communityPostRepository = communityPostRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsSuccess, List<CommunityCommentDTO>? Comments)> GetByPostIdAsync(int postId)
        {
            try
            {
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

        public async Task<(bool IsSuccess, CommunityCommentDTO? Comment)> GetByIdAsync(int id)
        {
            try
            {
                var comment = await _communityCommentRepository.GetByIdWithDetailsAsync(id);

                if (comment == null || comment.IsDeleted)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found or deleted", id);
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
            int commentId)
        {
            try
            {
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

        public async Task<(bool IsSuccess, CommunityCommentDTO? CreatedComment)> CreateAsync(AddCommunityComment model)
        {
            try
            {
                var post = await _communityPostRepository.GetByIdAsync(model.PostId);
                if (post == null || post.IsDeleted)
                {
                    _logger.LogWarning("Cannot create comment: Post with ID {PostId} not found or deleted",
                        model.PostId);
                    return (false, null);
                }

                if (model.ParentCommentId.HasValue)
                {
                    var parentComment = await _communityCommentRepository.GetByIdAsync(model.ParentCommentId.Value);
                    if (parentComment == null || parentComment.IsDeleted)
                    {
                        _logger.LogWarning(
                            "Cannot create reply: Parent comment with ID {ParentCommentId} not found or deleted",
                            model.ParentCommentId.Value);
                        return (false, null);
                    }

                    if (parentComment.PostId != model.PostId)
                    {
                        _logger.LogWarning("Parent comment {ParentCommentId} does not belong to post {PostId}",
                            model.ParentCommentId.Value, model.PostId);
                        return (false, null);
                    }
                }

                var comment = _mapper.Map<CommunityComment>(model);
                var createdComment = await _communityCommentRepository.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                if (createdComment != null)
                {
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
                        createdComment.Id, model.UserId);
                    return (true, commentDTO);
                }

                _logger.LogWarning("Failed to create comment for post {PostId}", model.PostId);
                return (false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId}", model.PostId);
                return (false, null);
            }
        }

        public async Task<bool> UpdateAsync(EditCommunityComment model)
        {
            try
            {
                var existingComment = await _communityCommentRepository.GetByIdAsync(model.Id);

                if (existingComment == null || existingComment.IsDeleted)
                {
                    _logger.LogWarning("Comment with ID {CommentId} not found or deleted", model.Id);
                    return false;
                }

                if (existingComment.UserId != model.UserId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to edit comment {CommentId}", model.UserId,
                        model.Id);
                    return false;
                }

                existingComment.Content = model.Content;
                existingComment.UpdatedOn = DateTime.UtcNow;
                existingComment.IsEdited = true;

                var result = await _communityCommentRepository.UpdateAsync(existingComment);
                await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    _logger.LogInformation("Comment {CommentId} updated successfully by user {UserId}", model.Id,
                        model.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", model.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
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
