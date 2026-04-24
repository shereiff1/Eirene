using Eirene.BLL.AIModel;
using Eirene.BLL.Extensions;
using Eirene.BLL.Hubs;
using Eirene.BLL.Mappers;
using Eirene.BLL.Models.Identity;
using Eirene.DAL.Database;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.PostgreSql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<SendGridSettings>(
    builder.Configuration.GetSection("SendGrid"));

builder.Services.AddAutoMapper(typeof(AuthProfile));

builder.Services.AddDbContext<EireneDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDataAccessServices()
                .AddBusinessLogicServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];

builder.Services.Configure<AIModelSettings>(
    builder.Configuration.GetSection("AIModel"));

builder.Services.Configure<PythonSettings>(
    builder.Configuration.GetSection("PythonSettings"));

builder.Services.AddSingleton<IPythonModelService, PythonModelService>();
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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

builder.Services.AddHangfireServer();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

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

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.UseHangfireDashboard("/hangfire");

app.Run();
