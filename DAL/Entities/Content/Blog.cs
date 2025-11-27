using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Entities.Core;

namespace DAL.Entities.Content
{
    public class Blog
    {
        [Key] public int Id { get; set; }
        [Required] public string DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))] public ApplicationUser Doctor { get; set; }

        [Required] public string BlogContent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}