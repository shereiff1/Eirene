

using BLL.ModelVMs.Content;

namespace BLL.Services.Abstraction.Content
{
    public interface IBlogServices
    {
        Task<(bool IsSuccess, List<BlogDTO>? Posts)> GetAllAsync();

        Task<(bool IsSuccess, BlogDTO? Post)> GetByIdAsync(Guid id);

        Task<(bool IsSuccess, BlogDTO? CreatedPost)> CreateAsync(AddBlog model, string doctorId);

        Task<bool> UpdateAsync(EditBlog model);

        Task<bool> DeleteAsync(Guid id);
    }
}
