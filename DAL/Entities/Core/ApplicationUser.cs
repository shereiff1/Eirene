using DAL.Entities.Community;
using Microsoft.AspNetCore.Identity;

namespace DAL.Entities.Core
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; } = false;
        public DoctorProfile? DoctorProfile { get; set; }
        public PatientProfile? PatientProfile { get; set; }
        public ModeratorProfile? ModeratorProfile { get; set; }
        public AdminProfile? AdminProfile { get; set; }
        public ICollection<CommunityGroup>? Groups { get; set; } = new List<CommunityGroup>();
    }
}