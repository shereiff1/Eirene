using DAL.Repository.Abstraction;
using DAL.Repository.Abstraction.Communication;
using DAL.Repository.Abstraction.Community;
using DAL.Repository.Abstraction.Content;
using DAL.Repository.Abstraction.Core;
using DAL.Repository.Abstraction.Tracking;
using DAL.Repository.Abstraction.Treatment;
using DAL.Repository.Implementation;
using DAL.Repository.Implementation.Communication;
using DAL.Repository.Implementation.Community;
using DAL.Repository.Implementation.Content;
using DAL.Repository.Implementation.Core;
using DAL.Repository.Implementation.Tracking;
using DAL.Repository.Implementation.Treatment;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Extensions;

public static class DalServicesExtensions
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<ICommunityGroupRepository, CommunityGroupRepository>();
        services.AddScoped<IJournalRepository, JournalRepository>();
        services.AddScoped<ICommunityCommentRepository, CommunityCommentRepository>();
        services.AddScoped<ICommunityPostRepository, CommunityPostRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IQuestionAnswerRepository, QuestionAnswerRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITreatmentPlanRepository, TreatmentPlanRepository>();
        services.AddScoped<IPatientTaskRepository, PatientTaskRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();

        return services;
    }
}