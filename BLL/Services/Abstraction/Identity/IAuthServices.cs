using BLL.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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