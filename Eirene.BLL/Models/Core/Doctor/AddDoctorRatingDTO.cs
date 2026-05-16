using System.ComponentModel.DataAnnotations;

namespace Eirene.BLL.Models.Core.Doctor
{
    public class AddDoctorRatingDTO
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Review { get; set; }
    }
}
