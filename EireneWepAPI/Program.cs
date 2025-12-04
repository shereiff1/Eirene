using BLL.Mappers;
using BLL.Models.Identity;
using BLL.Services.Abstraction.Content;
using BLL.Services.Abstraction.Identity;
using BLL.Services.Implementation.Content;
using BLL.Services.Implementation.identity;
using DAL.Database;
using DAL.Entities.Core;
using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Content;
using DAL.Repository.Abstraction.Core;
using DAL.Repository.Implementation;
using DAL.Repository.Implementation.Content;
using DAL.Repository.Implementation.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Eirene
{
    public class Program
    {
        public static void Main(string[] args)
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
            var app = builder.Build();

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