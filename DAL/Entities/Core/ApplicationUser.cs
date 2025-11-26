using Microsoft.AspNetCore.Identity;

namespace DAL.Entities.Core;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool IsEmailVerified { get; set; }


    public DoctorProfile? DoctorProfile { get; set; }
    public PatientProfile? PatientProfile { get; set; }
    public ModeratorProfile? ModeratorProfile { get; set; }
    public AdminProfile? AdminProfile { get; set; }
}