using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Communication;
using Eirene.DAL.Repository.Abstraction.Community;
using Eirene.DAL.Repository.Abstraction.Content;
using Eirene.DAL.Repository.Abstraction.Core;
using Eirene.DAL.Repository.Abstraction.Tracking;
using Eirene.DAL.Repository.Abstraction.Treatment;
using Eirene.DAL.Repository.Implementation;
using Eirene.DAL.Repository.Implementation.Communication;
using Eirene.DAL.Repository.Implementation.Community;
using Eirene.DAL.Repository.Implementation.Content;
using Eirene.DAL.Repository.Implementation.Core;
using Eirene.DAL.Repository.Implementation.Tracking;
using Eirene.DAL.Repository.Implementation.Treatment;
using Microsoft.Extensions.DependencyInjection;

namespace Eirene.DAL.Extensions;

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
        services.AddScoped<IDoctorProfileRepository, DoctorProfileRepository>();
        services.AddScoped<IPatientProfileRepository, PatientProfileRepository>();
        services.AddScoped<ISupervisionRequestRepository, SupervisionRequestRepository>();
        services.AddScoped<IDoctorRatingRepository, DoctorRatingRepository>();
        services.AddScoped<IAdminProfileRepository, AdminProfileRepository>();
        return services;
    }
}