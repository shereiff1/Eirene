using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Core.Patient;

public class AddPatientProfile
{
    [Required(ErrorMessage = "Date of Birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Emergency Contact is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string EmergencyContact { get; set; } = string.Empty;

    [Required(ErrorMessage = "Medical History is required")]
    [StringLength(2000, ErrorMessage = "Medical History cannot exceed 2000 characters")]
    public string MedicalHistory { get; set; } = string.Empty;
}