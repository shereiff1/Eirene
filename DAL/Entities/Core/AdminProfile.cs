namespace DAL.Entities.Core;

public class AdminProfile
{
    public string Id { get; set; }
    public ApplicationUser User { get; set; }

    public DateTime LastLogin { get; set; }
    public bool CanBanUsers { get; set; } = true;
}