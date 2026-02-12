using BLL.Services.Abstraction.Communication;
using BLL.Services.Abstraction.Community;
using BLL.Services.Abstraction.Content;
using BLL.Services.Abstraction.Identity;
using BLL.Services.Abstraction.Tracking;
using BLL.Services.Abstraction.Treatment;
using BLL.Services.Implementation.Communication;
using BLL.Services.Implementation.Community;
using BLL.Services.Implementation.Content;
using BLL.Services.Implementation.identity;
using BLL.Services.Implementation.Identity;
using BLL.Services.Implementation.Tracking;
using BLL.Services.Implementation.Treatment;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions;

public static class BllServicesExtensions
{
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
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
        return services;
    }
}