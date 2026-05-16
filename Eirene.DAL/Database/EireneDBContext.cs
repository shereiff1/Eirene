using Eirene.DAL.Entities.Communication;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Entities.Content;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Entities.Tracking;
using Eirene.DAL.Entities.Treatment;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eirene.DAL.Database;

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
    public DbSet<UserCommunityGroup> UserCommunityGroups { get; set; }
    public DbSet<CommunityPost> CommunityPosts { get; set; }
    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PatientTask> PatientTasks { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<SupervisionRequest> SupervisionRequests { get; set; }
    public DbSet<DoctorRating> DoctorRatings { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.AdminProfile)
            .WithOne(p => p.User)
            .HasForeignKey<AdminProfile>(p => p.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.DoctorProfile)
            .WithOne(p => p.User)
            .HasForeignKey<DoctorProfile>(p => p.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.PatientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PatientProfile>(p => p.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PatientProfile>()
            .HasOne(p => p.Doctor)
            .WithMany(d => d.Patients)
            .HasForeignKey(p => p.DoctorProfileId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Entity<PatientProfile>()
            .Property(p => p.DateOfBirth)
            .HasColumnType("date");

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

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Groups)
            .WithMany(g => g.Members)
            .UsingEntity<UserCommunityGroup>(
                right => right
                    .HasOne(ug => ug.CommunityGroup)
                    .WithMany(g => g.UserCommunityGroups)
                    .HasForeignKey(ug => ug.CommunityGroupId)
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne(ug => ug.User)
                    .WithMany(u => u.UserCommunityGroups)
                    .HasForeignKey(ug => ug.UserId)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("ApplicationUserCommunityGroup");
                    join.HasKey(ug => new { ug.CommunityGroupId, ug.UserId });

                    join.Property(ug => ug.CommunityGroupId)
                        .HasColumnName("GroupsId");

                    join.Property(ug => ug.UserId)
                        .HasColumnName("MembersId");

                    join.Property(ug => ug.IsBanned)
                        .HasDefaultValue(false);

                    join.Property(ug => ug.TimeoutUntil)
                        .HasColumnType("timestamp with time zone");
                });

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

        builder.Entity<SupervisionRequest>()
            .HasOne(r => r.Patient)
            .WithMany(p => p.SupervisionRequests)
            .HasForeignKey(r => r.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SupervisionRequest>()
            .HasOne(r => r.Doctor)
            .WithMany(d => d.SupervisionRequests)
            .HasForeignKey(r => r.DoctorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DoctorRating>()
            .HasOne(r => r.Doctor)
            .WithMany(d => d.DoctorRatings)
            .HasForeignKey(r => r.DoctorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DoctorRating>()
            .HasOne(r => r.Patient)
            .WithMany()
            .HasForeignKey(r => r.PatientProfileId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
