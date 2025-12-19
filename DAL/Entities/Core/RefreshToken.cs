using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Core;

public class RefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
}