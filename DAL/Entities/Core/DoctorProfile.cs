namespace DAL.Entities.Core
{
    public class DoctorProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        // Foreign Key
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public string Specialization { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Qualifications { get; set; } = string.Empty;
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }
    }
}
