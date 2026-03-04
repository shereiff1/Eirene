using AutoMapper;
using Eirene.BLL.Models.Core.Admin;
using Eirene.DAL.Entities.Core;

namespace Eirene.BLL.Mappers
{
    public class AdminProfileProfile : Profile
    {
        public AdminProfileProfile()
        {
            // Map Entity -> Model (for display)
            CreateMap<AdminProfile, AdminModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));
        }
    }
}
