using HasbitFlowApi.Data;
using HasbitFlowApi.DTOs.Auth;
using HasbitFlowApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HasbitFlowApi.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var EmailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (EmailExists)
            {
                //throw new InvalidOperationException("Email Already Exists");
                return false;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user,dto.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
