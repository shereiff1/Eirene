using DAL.Entities.Community;
using DAL.Entities.Content;
using DAL.Entities.Core;
using DAL.Entities.Tracking;
using DAL.Entities.Treatment;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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
    public DbSet<CommunityComment> CommunityComments { get; set; }
    public DbSet<CommunityGroup> CommunityGroups { get; set; }
    public DbSet<CommunityPost> CommunityPosts { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<Question> Questions { get; set; }
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
        // CommunityGroup configuration
        builder.Entity<CommunityGroup>()
            .HasOne(g => g.CreatedBy)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CommunityPost configuration
        builder.Entity<CommunityPost>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CommunityPost>()
            .HasOne(p => p.CommunityGroup)
            .WithMany(g => g.Posts)
            .HasForeignKey(p => p.CommunityGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // CommunityComment configuration
        builder.Entity<CommunityComment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CommunityComment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referential relationship for comment replies
        builder.Entity<CommunityComment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }


}