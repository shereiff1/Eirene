

using Eirene.BLL.Models.Common;
using Eirene.BLL.ModelVMs.Content;

namespace Eirene.BLL.Services.Abstraction.Content
{
    public interface IBlogServices
    {
        Task<(bool IsSuccess, PagedResult<BlogDTO>? Posts)> GetAllAsync(int page = 1, int pageSize = 10);

        Task<(bool IsSuccess, List<BlogDTO>? Posts)> GetByDoctorIdAsync(string doctorId);

        Task<(bool IsSuccess, BlogDTO? Post)> GetByIdAsync(Guid id);

        Task<(bool IsSuccess, BlogDTO? CreatedPost)> CreateAsync(AddBlog model, string doctorId);

        Task<bool> UpdateAsync(EditBlog model);

        Task<bool> DeleteAsync(Guid id);
    }
}
