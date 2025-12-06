using BLL.Models.Community.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Abstraction.Community
{
    public interface ICommunityPostServices
    {
        Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetAllAsync();
        Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByGroupIdAsync(int groupId);
        Task<(bool IsSuccess, CommunityPostDTO? Post)> GetByIdAsync(int id);
        Task<(bool IsSuccess, List<CommunityPostDTO>? Posts)> GetByUserIdAsync(string userId);
        Task<(bool IsSuccess, CommunityPostDTO? CreatedPost)> CreateAsync(AddCommunityPost model);
        Task<bool> UpdateAsync(EditCommunityPost model);
        Task<bool> DeleteAsync(int id);
    }
}
