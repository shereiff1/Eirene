using Eirene.BLL.Services.Abstraction.Background_Jobs;
using Eirene.BLL.Services.Abstraction.Communication;
using Eirene.BLL.Services.Abstraction.Community;
using Eirene.BLL.Services.Abstraction.Content;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.BLL.Services.Abstraction.Identity;
using Eirene.BLL.Services.Abstraction.Tracking;
using Eirene.BLL.Services.Abstraction.Treatment;
using Eirene.BLL.Services.Implementation.Background_Jobs;
using Eirene.BLL.Services.Implementation.Communication;
using Eirene.BLL.Services.Implementation.Community;
using Eirene.BLL.Services.Implementation.Content;
using Eirene.BLL.Services.Implementation.Core;
using Eirene.BLL.Services.Implementation.identity;
using Eirene.BLL.Services.Implementation.Identity;
using Eirene.BLL.Services.Implementation.Tracking;
using Eirene.BLL.Services.Implementation.Treatment;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

namespace Eirene.BLL.Extensions;

public static class BllServicesExtensions
{
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IBlogServices, BlogServices>();
        services.AddScoped<ICommunityGroupServices, CommunityGroupServices>();
        services.AddScoped<IJournalServices, JournalServices>();
        services.AddScoped<ICommunityCommentServices, CommunityCommentServices>();
        services.AddScoped<ICommunityPostServices, CommunityPostServices>();
        services.AddScoped<IQuestionServices, QuestionServices>();
        services.AddScoped<IQuestionAnswerServices, QuestionAnswerServices>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthServices, AuthServices>();
        services.AddScoped<ITreatmentPlanServices, TreatmentPlanServices>();
        services.AddScoped<IPatientTaskServices, PatientTaskServices>();
        services.AddScoped<IChatServices, ChatServices>();
        services.AddScoped<IDoctorServices, DoctorServices>();
        services.AddScoped<IPatientServices, PatientServices>();
        services.AddScoped<IAdminServices, AdminServices>();
        services.AddScoped<IBackgroundJobService, BackgroundJobServices>();

        var storageProvider = configuration["Storage:Provider"];
        if (storageProvider == "CloudinarySettings")
        {
            services.AddScoped<IPictureService, CloudImageStorage>();
        }
        else
        {
            services.AddScoped<IPictureService, LocalPictureService>();
        }

        return services;
    }
}