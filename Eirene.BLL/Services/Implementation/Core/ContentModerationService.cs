using Eirene.BLL.AIModel.Abstraction;
using Eirene.BLL.Services.Abstraction.Core;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core;


public class ContentModerationService : IContentModerationService
{
    private const double BanThreshold = 0.75;
    private const double TimeoutThreshold = 0.45;
    private static readonly TimeSpan DefaultTimeoutDuration = TimeSpan.FromHours(24);

    private readonly IToxicityService _toxicityService;
    private readonly ICommunityModerationService _communityModerationService;
    private readonly ILogger<ContentModerationService> _logger;

    public ContentModerationService(
        IToxicityService toxicityService,
        ICommunityModerationService communityModerationService,
        ILogger<ContentModerationService> logger)
    {
        _toxicityService = toxicityService;
        _communityModerationService = communityModerationService;
        _logger = logger;
    }

    public async Task<ContentModerationResult> ModerateAsync(string text, string userId, Guid communityGroupId)
    {

        var toxicity = await _toxicityService.AnalyseAsync(text);

        if (toxicity is null)
        {
            _logger.LogWarning(
                "Toxicity service unavailable – allowing content from user {UserId} without moderation",
                userId);
            return ContentModerationResult.Allowed();
        }

        _logger.LogInformation(
            "Toxicity analysis for user {UserId}: ViolationScore={ViolationScore}, Action={Action}",
            userId, toxicity.ViolationScore, toxicity.Action);

        if (toxicity.ViolationScore >= BanThreshold)
        {
            _logger.LogWarning(
                "High toxicity ({ViolationScore}) from user {UserId} in group {GroupId} – banning",
                toxicity.ViolationScore, userId, communityGroupId);

            var banResult = await _communityModerationService.BanUserFromGroupAsync(communityGroupId, userId);

            if (banResult.IsFailure)
            {
                _logger.LogWarning(
                    "Auto-ban failed for user {UserId} in group {GroupId}: {Error}",
                    userId, communityGroupId, banResult.Error);
            }

            return ContentModerationResult.Rejected(
                "Your message was flagged for severe toxicity. You have been banned from this community group.",
                toxicity);
        }

        if (toxicity.ViolationScore >= TimeoutThreshold)
        {
            _logger.LogWarning(
                "Moderate toxicity ({ViolationScore}) from user {UserId} in group {GroupId} – applying timeout",
                toxicity.ViolationScore, userId, communityGroupId);

            var timeoutUntil = DateTime.UtcNow.Add(DefaultTimeoutDuration);
            var timeoutResult = await _communityModerationService
                .TimeoutUserInGroupAsync(communityGroupId, userId, timeoutUntil);

            if (timeoutResult.IsFailure)
            {
                _logger.LogWarning(
                    "Auto-timeout failed for user {UserId} in group {GroupId}: {Error}",
                    userId, communityGroupId, timeoutResult.Error);
            }

            return ContentModerationResult.Rejected(
                "Your message was flagged for toxic content. You have been timed out for 24 hours.",
                toxicity);
        }

        return ContentModerationResult.Allowed(toxicity);
    }
}
