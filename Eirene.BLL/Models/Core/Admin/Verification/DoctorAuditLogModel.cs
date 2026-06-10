namespace Eirene.BLL.Models.Core.Admin.Verification
{
    public class DoctorAuditLogModel
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
