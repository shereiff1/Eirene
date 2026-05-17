using AutoMapper;
using Eirene.BLL.Models.Core.Doctor;
using Eirene.DAL.Entities.Core;

namespace Eirene.BLL.Mappers
{
    public class DoctorProfileProfile : Profile
    {
        public DoctorProfileProfile()
        {
            // Map Entity -> Model (for display)
            CreateMap<DoctorProfile, DoctorModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.PatientCount, opt => opt.MapFrom(src => src.Patients.Count));

            // Map AddModel -> Entity (creation)
            CreateMap<AddDoctorProfile, DoctorProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewCount, opt => opt.Ignore())
                .ForMember(dest => dest.JoinedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Map EditModel -> Entity (update)
            CreateMap<EditDoctorProfile, DoctorProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewCount, opt => opt.Ignore())
                .ForMember(dest => dest.JoinedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<SupervisionRequest, SupervisionRequestDTO>()
                .ForMember(dest => dest.PatientFullName, opt => opt.MapFrom(src => src.Patient.User.FullName))
                .ForMember(dest => dest.PatientProfilePhotoUrl, opt => opt.MapFrom(src => src.Patient.ProfilePhotoUrl));

            CreateMap<SupervisionRequest, DoctorPatientDTO>()
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientProfileId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Patient.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Patient.User.Email))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Patient.DateOfBirth))
                .ForMember(dest => dest.ProfilePhotoUrl, opt => opt.MapFrom(src => src.Patient.ProfilePhotoUrl))
                .ForMember(dest => dest.AcceptedAt, opt => opt.MapFrom(src => src.RespondedAt ?? src.CreatedAt));
        }
    }
}
