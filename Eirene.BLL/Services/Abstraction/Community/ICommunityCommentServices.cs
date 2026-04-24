using Eirene.BLL.Models.Community.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirene.BLL.Services.Abstraction.Community
{
    public interface ICommunityCommentServices
    {
        Task<(bool IsSuccess, List<CommunityCommentDTO>? Comments)> GetByPostIdAsync(Guid postId);
        Task<(bool IsSuccess, CommunityCommentDTO? Comment)> GetByIdAsync(Guid id);
        Task<(bool IsSuccess, List<CommunityCommentDTO>? Replies)> GetRepliesByCommentIdAsync(Guid commentId);
        Task<(bool IsSuccess, string Message, CommunityCommentDTO? CreatedComment)> CreateAsync(AddCommunityComment model);
        Task<bool> UpdateAsync(EditCommunityComment model);
        Task<bool> DeleteAsync(Guid id);
    }
}
