using ClinicHub.DTOs.Account;
using ClinicHub.Models.Clinic;

namespace ClinicHub.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User?> LoginAsync(LoginDto dto);
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<bool> CreateAdminAsync(CreateAdminDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}
