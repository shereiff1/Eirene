// using System.ComponentModel.DataAnnotations;
//
// namespace DAL.Entities.Core;
//
// public class Rating
// {
//     public Guid Id { get; set; } = Guid.NewGuid();
//     
//     public string PatientProfileId { get; set; } = string.Empty;
//     public PatientProfile Patient { get; set; } = null!;
//
//     public string DoctorProfileId { get; set; } = string.Empty;
//     public DoctorProfile Doctor { get; set; } = null!;
//     
//     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     public DateTime? RespondedAt { get; set; }
//     
// }