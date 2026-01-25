using AutoMapper;
using BLL.Models.Identity;
using DAL.Entities.Core;

namespace BLL.Mappers
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                // .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => 0))
                
                // Ignore Identity properties that will be managed by UserManager
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                
                // Ignore navigation properties
                .ForMember(dest => dest.DoctorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.PatientProfile, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.AdminProfile, opt => opt.Ignore());

            CreateMap<ApplicationUser, AuthResultDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Success, opt => opt.Ignore())
                .ForMember(dest => dest.Message, opt => opt.Ignore())
                .ForMember(dest => dest.Error, opt => opt.Ignore());


            CreateMap<ResetPasswordDTO, ApplicationUser>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                
                // Ignore all other properties as ResetPasswordDTO only contains password info
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.PatientProfile, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.AdminProfile, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore());

            // Optional: CreateMap<LoginDTO, ApplicationUser> if needed
            // This mapping might not be necessary since LoginDTO only contains email/password
            /*
            CreateMap<LoginDTO, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.PatientProfile, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.AdminProfile, opt => opt.Ignore());
            */
        }
    }
}