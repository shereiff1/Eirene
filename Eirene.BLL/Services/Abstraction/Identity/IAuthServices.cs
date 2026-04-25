using Eirene.BLL.Models.Identity;

namespace Eirene.BLL.Services.Abstraction.Identity
{
    public interface IAuthServices
    {
        Task<RegistrationDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResultDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResultDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDto);
        Task<ConfirmMailDTO> ConfirmEmailCodeAsync(string Email, string ConfirmationCode);
        Task LogoutAsync(string userId);
        Task<AuthResultDTO> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<AuthResultDTO> ResendVerificationCodeAsync(string Email);
        Task<AuthResultDTO> ForgotPasswordAsync(ForgotPasswordDTO dto);
        Task<AuthResultDTO> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}
