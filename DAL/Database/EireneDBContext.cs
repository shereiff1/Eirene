using DAL.Entities.Content;
using DAL.Entities.Core;
using DAL.Entities.Tracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Database;

public class EireneDBContext : IdentityDbContext<ApplicationUser>
{
    public EireneDBContext(DbContextOptions<EireneDBContext> options)
        : base(options)
    {
    }
    public DbSet<DoctorProfile> DoctorProfiles { get; set; }
    public DbSet<PatientProfile> PatientProfiles { get; set; }
    public DbSet<ModeratorProfile> ModeratorProfiles { get; set; }
    public DbSet<AdminProfile> AdminProfiles { get; set; }
    public DbSet<Journal> Journals { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Admin Profile
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.AdminProfile)
            .WithOne(p => p.User)
            .HasForeignKey<AdminProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor Profile
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.DoctorProfile)
            .WithOne(p => p.User)
            .HasForeignKey<DoctorProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Patient Profile
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.PatientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PatientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Moderator Profile
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.ModeratorProfile)
            .WithOne(p => p.User)
            .HasForeignKey<ModeratorProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }


}