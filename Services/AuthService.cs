using LoginService.Models;
using LoginService.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LoginService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IMemoryCache cache, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _cache = cache;
            _configuration = configuration;
        }

        public string Authenticate(string username, string password)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user == null) return null;

            if (!VerifyPassword(password, user.PasswordHash, user.Salt))
                return null;

            string token = GenerateJwtToken(user);

            // Cache token with a session entry
            var session = new Session
            {
                Token = token,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Set expiration time
                IsValid = true
            };

            _userRepository.AddSession(session);
            _cache.Set(token, session, TimeSpan.FromHours(1));

            return token;
        }

        public bool ValidateSession(string token)
        {
            if (_cache.TryGetValue(token, out Session session))
                return session.IsValid;

            session = _userRepository.GetSessionByToken(token);
            if (session == null || !session.IsValid) return false;

            // Refresh cache
            _cache.Set(token, session, TimeSpan.FromHours(1));
            return true;
        }

        public void Logout(string token)
        {
            _cache.Remove(token);
            _userRepository.InvalidateSession(token);
        }

        public static bool VerifyPassword(string providedPassword, string storedHash, string salt)
        {
            // Hash the provided password with the salt
            var hashedPassword = HashPassword(providedPassword, salt);

            // Compare the hashed password with the stored hash
            return hashedPassword == storedHash;
        }

        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                // Combine the password and salt
                var combined = Encoding.UTF8.GetBytes(password + salt);

                // Compute the hash
                var hash = sha256.ComputeHash(combined);

                // Convert to Base64 string
                return Convert.ToBase64String(hash);
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"]);

            // Ensure the key is at least 32 bytes (256 bits)
            if (Encoding.UTF8.GetBytes(key).Length < 32)
            {
                throw new ArgumentOutOfRangeException("JwtSettings:Key", "The JWT key must be at least 32 bytes (256 bits).");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
