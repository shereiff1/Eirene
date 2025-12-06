using BLL.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BLL.Services.Abstraction.Community;
using BLL.Services.Abstraction.Content;
using BLL.Services.Abstraction.Identity;
using BLL.Services.Abstraction.Tracking;
using BLL.Services.Implementation.Community;
using BLL.Services.Implementation.Content;
using BLL.Services.Implementation.identity;
using BLL.Services.Implementation.Tracking;
using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Content;
using DAL.Repository.Abstraction.Core;
using DAL.Repository.Abstraction.Tracking;
using DAL.Repository.Implementation;
using DAL.Repository.Implementation.Community;
using DAL.Repository.Implementation.Content;
using DAL.Repository.Implementation.Core;
using DAL.Repository.Implementation.Tracking;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Eirene
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.Configure<SmtpSettings>(
                builder.Configuration.GetSection("Smtp"));
            builder.Services.AddAutoMapper(typeof(AuthProfile));
            builder.Services.AddDbContext<EireneDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = false;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<EireneDBContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            builder.Services.AddScoped<IAuthServices, AuthServices>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IBlogServices, BlogServices>();
            builder.Services.AddScoped<IBlogRepository, BlogRepository>();
            builder.Services.AddScoped<ICommunityGroupRepository, CommunityGroupRepository>();
            builder.Services.AddScoped<ICommunityGroupServices, CommunityGroupServices>();
            builder.Services.AddScoped<IJournalServices, JournalServices>();
            builder.Services.AddScoped<IJournalRepository, JournalRepository>();
            builder.Services.AddScoped<ICommunityCommentServices, CommunityCommentServices>();
            builder.Services.AddScoped<ICommunityCommentRepository, CommunityCommentRepository>();
            builder.Services.AddScoped<ICommunityPostRepository, CommunityPostRepository>();
            builder.Services.AddScoped<ICommunityPostServices, CommunityPostServices>();
            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Add Authorization
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Seed roles on startup
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Patient", "Doctor", "Moderator", "Admin" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }



            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
