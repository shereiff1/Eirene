using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Core.Doctor;

public class EditDoctorProfile
{
    // [Required(ErrorMessage = "Biography is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Biography must be between 50 and 2000 characters")]
    public string? Biography { get; set; } = null!;

    [Phone(ErrorMessage = "Invalid phone number format")]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in valid international format")]
    public string? PhoneNumber { get; set; } = null!;

    [Range(0, 70, ErrorMessage = "Years of experience must be between 0 and 70")]
    public int? YearsOfExperience { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Qualifications cannot exceed 1000 characters")]
    public string? Qualifications { get; set; } = null!;

    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
    public string? ProfilePhotoUrl { get; set; }
}