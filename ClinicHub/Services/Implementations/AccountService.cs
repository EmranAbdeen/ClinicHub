using ClinicHub.DTOs.Account;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using ClinicHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly ClinicDbContext db;
        public AccountService(ClinicDbContext db)
        {
            this.db = db;
        }

        public async Task<User?> LoginAsync(LoginDto dto)
        {
            var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!passwordValid)
            {
                return null;
            }

            return user;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var emailExists = await db.Users.AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                return false;

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Patient");

            if (role == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return false;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.PhoneNumber,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.Now,
                RoleId = role.Id,
                Patient = new Patient
                {
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender
                }
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            bool currentPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!currentPasswordValid)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CreateAdminAsync(CreateAdminDto dto)
        {
            var emailExists = await db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                return false;

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (role == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return false;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.PhoneNumber,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.Now,
                RoleId = role.Id
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return true;
        }
    }
}