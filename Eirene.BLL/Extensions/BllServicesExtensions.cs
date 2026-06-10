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
        services.AddScoped<IEmailSender, SendGridEmailSender>();
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
        services.AddScoped<IDoctorProfileService, DoctorProfileService>();
        services.AddScoped<ISupervisionService, SupervisionService>();
        services.AddScoped<IDoctorRatingService, DoctorRatingService>();
        services.AddScoped<IAdminProfileService, AdminProfileService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<ICommunityModerationService, CommunityModerationService>();
        services.AddScoped<IContentModerationService, ContentModerationService>();
        services.AddScoped<IPatientServices, PatientServices>();
        services.AddScoped<IBackgroundJobService, BackgroundJobServices>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IDoctorVerificationService, DoctorVerificationService>();

        var storageProvider = configuration["Storage:Provider"];
        if (storageProvider == "CloudinarySettings")
        {
            services.AddScoped<IPictureService, CloudImageStorage>();
            services.AddScoped<IDocumentStorageService, CloudDocumentStorageService>();
        }
        else
        {
            services.AddScoped<IPictureService, LocalPictureService>();
            // If they fall back to local, use a dummy or implement local document storage later. For now, CloudDocumentStorageService requires Cloudinary. 
            // We'll just map it to CloudDocumentStorageService anyway, or create a quick LocalDocumentStorageService. Let's just use CloudDocumentStorageService for now or throw if used.
            // But since they explicitly want Cloudinary for documents due to Railway, we'll register the cloud one.
            services.AddScoped<IDocumentStorageService, LocalDocumentStorageService>();
        }

        return services;
    }
}
