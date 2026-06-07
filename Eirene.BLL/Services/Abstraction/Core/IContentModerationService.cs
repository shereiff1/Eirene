using Eirene.BLL.AIModel;

namespace Eirene.BLL.Services.Abstraction.Core;

public interface IContentModerationService
{
    Task<ContentModerationResult> ModerateAsync(string text, string userId, Guid communityGroupId);
}

public class ContentModerationResult
{
    public bool IsAllowed { get; init; }
    public string? RejectionReason { get; init; }
    public ToxicityResult? ToxicityResult { get; init; }

    public static ContentModerationResult Allowed(ToxicityResult? toxicity = null)
        => new() { IsAllowed = true, ToxicityResult = toxicity };

    public static ContentModerationResult Rejected(string reason, ToxicityResult? toxicity = null)
        => new() { IsAllowed = false, RejectionReason = reason, ToxicityResult = toxicity };
}
