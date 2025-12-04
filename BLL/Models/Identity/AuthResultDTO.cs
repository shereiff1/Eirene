namespace BLL.Models.Identity
{
    public class AuthResultDTO
    {
        public bool Success { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
    }
}
