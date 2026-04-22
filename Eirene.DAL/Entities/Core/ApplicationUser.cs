using Eirene.DAL.Entities.Community;
using Microsoft.AspNetCore.Identity;

namespace Eirene.DAL.Entities.Core
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; } = false;
        public  string EmailVerificationCode { get; set; } = string.Empty;
        public DateTime EmailVerificationCodeExpiration { get; set; }
        public DoctorProfile? DoctorProfile { get; set; }
        public PatientProfile? PatientProfile { get; set; }
        public ModeratorProfile? ModeratorProfile { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
        public AdminProfile? AdminProfile { get; set; }
        public ICollection<CommunityGroup>? Groups { get; set; } = new List<CommunityGroup>();
        public ICollection<UserCommunityGroup>? UserCommunityGroups { get; set; } = new List<UserCommunityGroup>();
    }
}
