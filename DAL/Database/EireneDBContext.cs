using DAL.Entities.Communication;
using DAL.Entities.Community;
using DAL.Entities.Content;
using DAL.Entities.Core;
using DAL.Entities.Tracking;
using DAL.Entities.Treatment;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Database;

public class EireneDBContext : IdentityDbContext<ApplicationUser>
{
    public EireneDBContext(DbContextOptions options) : base(options)
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
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PatientTask> PatientTasks { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.AdminProfile)
            .WithOne(p => p.User)
            .HasForeignKey<AdminProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.DoctorProfile)
            .WithOne(p => p.User)
            .HasForeignKey<DoctorProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.PatientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PatientProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PatientProfile>()
            .HasOne(p => p.Doctor)
            .WithMany(d => d.Patients)
            .HasForeignKey(p => p.DoctorProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.ModeratorProfile)
            .WithOne(p => p.User)
            .HasForeignKey<ModeratorProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CommunityGroup>()
            .HasOne(g => g.CreatedBy)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Entity<CommunityComment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TreatmentPlan>()
            .HasOne(tp => tp.User)
            .WithMany()
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TreatmentPlan>()
            .HasMany(tp => tp.Tasks)
            .WithOne(t => t.TreatmentPlan)
            .HasForeignKey(t => t.TreatmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PatientTask>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}