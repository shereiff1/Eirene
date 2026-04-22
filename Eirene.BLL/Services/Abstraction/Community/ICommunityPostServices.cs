using Eirene.BLL.Models.Community.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirene.BLL.Services.Abstraction.Community
{
    public interface ICommunityPostServices
    {
        Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetAllAsync();
        Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByGroupIdAsync(Guid groupId);
        Task<(bool IsSuccess, CommunityPostDTO? Post)> GetByIdAsync(Guid id);
        Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByUserIdAsync(string userId);
        Task<(bool IsSuccess, string Message, CommunityPostDTO? CreatedPost)> CreateAsync(AddCommunityPost model);
        Task<bool> UpdateAsync(EditCommunityPost model);
        Task<bool> DeleteAsync(Guid id);
    }
}
