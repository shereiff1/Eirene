using BLL.AIModel;
using BLL.Extensions;
using BLL.Mappers;
using BLL.Models.Identity;
using DAL.Database;
using DAL.Entities.Core;
using DAL.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            builder.Services.AddDataAccessServices().AddBusinessLogicServices();
            builder.Services.AddHttpContextAccessor();
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];
            builder.Services.Configure<AIModelSettings>(
                builder.Configuration.GetSection("AIModel"));
            builder.Services.AddHttpClient<IAIModelService, AIModelService>();
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

            app.UseAuthentication(); 
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}