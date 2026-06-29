using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.ModelVMs.Content;
using Eirene.BLL.Services.Abstraction.Content;
using Eirene.DAL.Entities.Content;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Content;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Content
{
    public class BlogServices : IBlogServices
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BlogServices> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public BlogServices(IBlogRepository blogRepository, ILogger<BlogServices> logger, IMapper mapper, IUnitOfWork unitOfWork, HybridCache cache)
        {
            _blogRepository = blogRepository;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<(bool IsSuccess, PagedResult<BlogDTO>? Posts)> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await _blogRepository.GetAllPagedAsync(page, pageSize);
                if (result.Items == null) return (false, null);

                var blogDtOs = _mapper.Map<List<BlogDTO>>(result.Items);
                
                var pagedResult = new PagedResult<BlogDTO>
                {
                    Items = blogDtOs,
                    TotalCount = result.TotalCount,
                    Page = page,
                    PageSize = pageSize
                };
                
                return (true, pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, List<BlogDTO>? Posts)> GetByDoctorIdAsync(string doctorId)
        {
            try
            {
                var cacheKey = $"doctor-blogs-{doctorId}";
                var blogDtOs = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async token =>
                    {
                        var blogs = await _blogRepository.FindAsync(b => b.DoctorId == doctorId);
                        return _mapper.Map<List<BlogDTO>>(blogs);
                    }
                );

                if (blogDtOs == null) return (false, null);

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
                var cacheKey = $"blog-{id}";
                var blogDtO = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async token =>
                    {
                        var blog = await _blogRepository.GetByIdAsync(id);
                        return _mapper.Map<BlogDTO>(blog);
                    }
                );

                if (blogDtO == null) return (false, null);
                return (true, blogDtO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return (false, null);
            }
        }

        public async Task<(bool IsSuccess, BlogDTO? CreatedPost)> CreateAsync(AddBlog model, string doctorId)
        {
            try
            {
                var blog = _mapper.Map<Blog>(model);
                blog.DoctorId = doctorId;

                var created = await _blogRepository.AddAsync(blog);
                await _unitOfWork.SaveChangesAsync();

                if (created == null)
                    return (false, null);

                await _cache.RemoveAsync($"doctor-blogs-{doctorId}");

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

                if (result)
                {
                    await _cache.RemoveAsync($"blog-{model.Id}");
                    await _cache.RemoveAsync($"doctor-blogs-{model.DoctorId}");
                }

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

                var doctorId = blog.DoctorId;
                var deleted = await _blogRepository.DeleteAsync(blog);
                await _unitOfWork.SaveChangesAsync();

                if (deleted)
                {
                    await _cache.RemoveAsync($"blog-{id}");
                    await _cache.RemoveAsync($"doctor-blogs-{doctorId}");
                }

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
