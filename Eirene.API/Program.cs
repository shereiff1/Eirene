using Eirene.BLL.AIModel;
using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.AIModel.Implementation;
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
using Eirene.API.Filters;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();

builder.Services.Configure<SendGridSettings>(
    builder.Configuration.GetSection("SendGrid"));

builder.Services.AddAutoMapper(typeof(AuthProfile));

builder.Services.AddDbContextPool<EireneDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<EireneDBContext>()
.AddDefaultTokenProviders();

builder.Services.AddDataAccessServices()
                .AddBusinessLogicServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];

builder.Services.Configure<AISettings>(
    builder.Configuration.GetSection("AIModel"));

builder.Services.AddHttpClient<IPythonModelService, PythonModelService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IToxicityService, ToxicityService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IAIModelService, AIModelService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient<IChatbotApiClient, ChatbotApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
});

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
    // for signalR (signalR do not have haeders)
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
}) // use cookie-based login instead of JWT, username/password page rather than JWT token.
.AddCookie("HangfireCookie", options =>
{
    options.LoginPath = "/hangfire-login";
    options.Cookie.Name = "HangfireAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
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

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
if (string.IsNullOrEmpty(redisConnectionString))
{
    redisConnectionString = "localhost:6379"; // Default for development if not provided
}
ConfigurationOptions redisOptions;
if (redisConnectionString != null && redisConnectionString.StartsWith("redis://"))
{
    var uri = new Uri(redisConnectionString);
    var host = uri.Host;
    var redisPort = uri.Port;
    var userInfo = uri.UserInfo.Split(':');
    var password = userInfo.Length > 1 ? userInfo[1] : userInfo[0];
    var user = userInfo.Length > 1 ? userInfo[0] : null;

    redisOptions = new ConfigurationOptions
    {
        EndPoints = { { host, redisPort } },
        Password = password,
        User = user,
        AbortOnConnectFail = false,
        Ssl = uri.Scheme == "rediss"
    };
}
else
{
    redisOptions = ConfigurationOptions.Parse(redisConnectionString!);
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisOptions;
    options.InstanceName = "Eirene:";
});
builder.Services.AddHybridCache();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

if (!app.Environment.IsEnvironment("Testing"))
{
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
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    IgnoreAntiforgeryToken = true
});

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.MapGet("/api/daily-wisdom", async (IConfiguration config, IHttpClientFactory httpClientFactory) =>
{
    var apiKey = config["NinjaApiKey"];
    if (string.IsNullOrEmpty(apiKey))
        return Results.StatusCode(503);

    var client = httpClientFactory.CreateClient();
    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.api-ninjas.com/v2/quoteoftheday");
    request.Headers.Add("X-Api-Key", apiKey);

    var response = await client.SendAsync(request);
    if (!response.IsSuccessStatusCode)
        return Results.StatusCode(502);

    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json");
}).RequireAuthorization();


app.Run();

public partial class Program { }
