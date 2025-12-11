using BLL.Models.Identity;
 

namespace BLL.Services.Abstraction.Identity
{
    public interface IAuthServices
    {
        Task<AuthResultDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResultDTO> LoginAsync(LoginDTO loginDto);
        public Task<AuthResultDTO> ConfirmEmailCodeAsync(ConfirmEmailCode dto);
        Task LogoutAsync(string userId);
    }
}
