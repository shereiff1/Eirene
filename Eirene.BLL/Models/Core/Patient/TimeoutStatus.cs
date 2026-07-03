namespace Eirene.BLL.Models.Core.Patient
{
    /// <summary>
    /// Carries the timeout state for a patient within a community group,
    /// returned as the value inside a <see cref="Eirene.BLL.Models.Common.Result{T}"/>.
    /// </summary>
    public class TimeoutStatus
    {
        public bool IsTimedOut { get; init; }
        public DateTime? TimeoutUntil { get; init; }
    }
}
