using BLL.Models.Community.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Abstraction.Community
{
    public interface ICommunityCommentServices
    {
        Task<(bool IsSuccess, List<CommunityCommentDTO>? Comments)> GetByPostIdAsync(int postId);
        Task<(bool IsSuccess, CommunityCommentDTO? Comment)> GetByIdAsync(int id);
        Task<(bool IsSuccess, List<CommunityCommentDTO>? Replies)> GetRepliesByCommentIdAsync(int commentId);
        Task<(bool IsSuccess, CommunityCommentDTO? CreatedComment)> CreateAsync(AddCommunityComment model);
        Task<bool> UpdateAsync(EditCommunityComment model);
        Task<bool> DeleteAsync(int id);
    }
}
