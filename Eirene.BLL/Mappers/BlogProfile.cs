using AutoMapper;
using BLL.ModelVMs.Content;
using DAL.Entities.Content;

namespace BLL.Mappers
{
    public class BlogProfile : Profile
    {
        public BlogProfile()
        {
            CreateMap<AddBlog, Blog>();
            CreateMap<EditBlog, Blog>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Blog, BlogDTO>();
        }
    }
}