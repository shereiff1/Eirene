namespace DAL.Entities.Core;

public class DoctorProfile
{
    public string Id { get; set; }
    public ApplicationUser User { get; set; }

    public string? Qualifications { get; set; }
    public double Rating { get; set; }
    public int PatientsCount { get; set; }
    public string PhoneNumber { get; set; }
    
    
    public ICollection<PatientProfile> Patients { get; set; }
}