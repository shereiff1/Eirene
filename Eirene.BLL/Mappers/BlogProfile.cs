using AutoMapper;
using Eirene.BLL.ModelVMs.Content;
using Eirene.DAL.Entities.Content;

namespace Eirene.BLL.Mappers
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