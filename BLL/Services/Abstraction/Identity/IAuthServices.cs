using BLL.Models.Identity;

namespace BLL.Services.Abstraction.Identity
{
    public interface IAuthServices
    {
        Task<RegistrationDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResultDTO> LoginAsync(LoginDTO loginDto);
        Task<ConfirmMailDTO> ConfirmEmailCodeAsync(ConfirmEmailCode dto);
        Task LogoutAsync(string userId);
        Task<AuthResultDTO> RefreshTokenAsync(string accessToken, string refreshToken); 
        Task<AuthResultDTO> ResendVerificationCodeAsync(string email);
    }
}