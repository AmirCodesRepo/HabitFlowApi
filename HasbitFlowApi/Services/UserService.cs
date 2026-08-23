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
        private readonly JwtService _jwtService;

        public UserService(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _jwtService = jwtService;
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

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenHash = _jwtService.HashToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenHash = _jwtService.HashToken(refreshToken);

            var tokenEntitiy = await _context.RefreshTokens.Include(t => t.User)
                                .FirstOrDefaultAsync(t => t.TokenHash == refreshTokenHash);

            if (tokenEntitiy is null)
                return null;
            
            if (tokenEntitiy.ExpiresAt <= DateTime.UtcNow)
                return null;

            if (tokenEntitiy.RevokedAt is not null)
                return null;

            var accessToken = _jwtService.GenerateToken(tokenEntitiy.User);

            tokenEntitiy.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newRefreshTokenHash = _jwtService.HashToken(newRefreshToken);
            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = tokenEntitiy.UserId
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenHash = _jwtService.HashToken(refreshToken);

            var tokneEntity = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshTokenHash);

            if (tokneEntity is null)
                return false;
            if (tokneEntity.RevokedAt is not null)
                return false;

            tokneEntity.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
