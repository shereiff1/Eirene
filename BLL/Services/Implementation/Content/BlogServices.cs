using AutoMapper;
using BLL.ModelVMs.Content;
using BLL.Services.Abstraction.Content;
using DAL.Entities.Content;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Content;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Implementation.Content
{
    public class BlogServices : IBlogServices
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BlogServices> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public BlogServices(IBlogRepository blogRepository, ILogger<BlogServices> logger, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _blogRepository = blogRepository;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsSuccess, List<BlogDTO>? Posts)> GetAllAsync()
        {
            try
            {
                var blogs = await _blogRepository.GetAllAsync();
                if (blogs == null) return (false, null);

                var blogDtOs = _mapper.Map<List<BlogDTO>>(blogs);
                return (true, blogDtOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, BlogDTO? Post)> GetByIdAsync(Guid id)
        {
            try
            {
                var blog = await _blogRepository.GetByIdAsync(id);
                if (blog == null) return (false, null);
                var blogDtO = _mapper.Map<BlogDTO>(blog);
                return (true, blogDtO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, BlogDTO? CreatedPost)> CreateAsync(AddBlog model)
        {
            try
            {
                var blog = _mapper.Map<Blog>(model);

                var created = await _blogRepository.AddAsync(blog);
                await _unitOfWork.SaveChangesAsync();

                if (created == null)
                    return (false, null);

                var dto = _mapper.Map<BlogDTO>(created);

                return (true, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<bool> UpdateAsync(EditBlog model)
        {
            try
            {
                var blog = await _blogRepository.GetByIdAsync(model.Id);
                if (blog == null) return false;
                if (blog.DoctorId != model.DoctorId) return false;
                _mapper.Map(model, blog);

                var result = await _blogRepository.UpdateAsync(blog);
                await _unitOfWork.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var blog = await _blogRepository.GetByIdAsync(id);
                if (blog == null) return false;

                var deleted = await _blogRepository.DeleteAsync(blog);
                await _unitOfWork.SaveChangesAsync();

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
