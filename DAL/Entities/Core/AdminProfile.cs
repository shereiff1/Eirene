namespace DAL.Entities.Core
{
    public class AdminProfile
    {
        public string Id { get; set; } = string.Empty;

        // REQUIRED foreign key
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public DateTime LastLogin { get; set; }
        public bool CanBanUsers { get; set; } = true;
    }
}
