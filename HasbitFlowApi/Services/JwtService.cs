using HasbitFlowApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HasbitFlowApi.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(token);

            var hash = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hash);
        }
        public string GenerateToken(User user)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt Key is not configured");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt Issuer is not configured");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt Audience is not configured");

            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]
                ?? "60");

            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Sub ,user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email ,user.Email),
                new Claim(ClaimTypes.Name ,user.Name),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer : issuer,
                audience : audience,
                claims : claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials : credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
