using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Core.Patient;

public class EditPatientProfile
{
    [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? EmergencyContact { get; set; }

    [StringLength(2000, ErrorMessage = "Medical History cannot exceed 2000 characters")]
    public string? MedicalHistory { get; set; }
    
    [RegularExpression(@"^https?:\/\/[^\s/$.?#].[^\s]*$",
        ErrorMessage = "Invalid URL.")]
    public string? ProfilePhotoUrl { get; set; }
}