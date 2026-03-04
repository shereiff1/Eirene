using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Core
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TokenHash { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedDate { get; set; }
    }
}