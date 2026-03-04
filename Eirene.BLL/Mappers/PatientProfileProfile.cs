using AutoMapper;
using Eirene.BLL.Models.Core.Patient;
using Eirene.DAL.Entities.Core;

namespace Eirene.BLL.Mappers
{
    public class PatientProfileProfile : Profile
    {
        public PatientProfileProfile()
        {
            // Map Entity -> Model (for display)
            CreateMap<PatientProfile, PatientModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.dateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth));

            // Map AddModel -> Entity (creation)
            CreateMap<AddPatientProfile, PatientProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.Journals, opt => opt.Ignore())
                .ForMember(dest => dest.MoodTrackers, opt => opt.Ignore())
                .ForMember(dest => dest.TreatmentPlans, opt => opt.Ignore())
                .ForMember(dest => dest.Diagnoses, opt => opt.Ignore())
                .ForMember(dest => dest.SupervisionRequests, opt => opt.Ignore());

            // Map EditModel -> Entity (update)
            CreateMap<EditPatientProfile, PatientProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
                .ForMember(dest => dest.Journals, opt => opt.Ignore())
                .ForMember(dest => dest.MoodTrackers, opt => opt.Ignore())
                .ForMember(dest => dest.TreatmentPlans, opt => opt.Ignore())
                .ForMember(dest => dest.Diagnoses, opt => opt.Ignore())
                .ForMember(dest => dest.SupervisionRequests, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
