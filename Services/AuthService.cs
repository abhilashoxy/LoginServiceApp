using LoginService.Models;
using LoginService.Repositories;
using Microsoft.Extensions.Caching.Memory;
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

        public AuthService(IUserRepository userRepository, IMemoryCache cache)
        {
            _userRepository = userRepository;
            _cache = cache;
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

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("YourSecretKey");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }


}
