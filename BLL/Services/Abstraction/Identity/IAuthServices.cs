using BLL.Models.Identity;

namespace BLL.Services.Abstraction.Identity
{
    public interface IAuthServices
    {
        Task<AuthResultDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResultDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResultDTO> ConfirmEmailCodeAsync(ConfirmEmailCode dto);
        Task LogoutAsync(string userId);
        Task<AuthResultDTO> RefreshTokenAsync(string accessToken, string refreshToken); // Added
    }
}