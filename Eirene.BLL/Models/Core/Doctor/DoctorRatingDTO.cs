namespace Eirene.BLL.Models.Core.Doctor
{
    public class DoctorRatingDTO
    {
        public string Id { get; set; } = string.Empty;
        public string PatientProfileId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Review { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
