using LoginService.Models;
using LoginService.Repositories;

namespace LoginService.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string RegisterUser(string username, string email, string password)
        {
            // Check if username or email already exists
            if (_userRepository.GetUserByUsername(username) != null)
            {
                return "Username already exists.";
            }

            if (_userRepository.GetUserByEmail(email) != null)
            {
                return "Email already exists.";
            }

            // Generate salt and hash password
            var salt = PasswordHelper.GenerateSalt();
            var hashedPassword = PasswordHelper.HashPassword(password, salt);

            // Create and save the user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                Salt = salt,
                CreatedDate = DateTime.UtcNow
            };

            _userRepository.AddUser(user);

            return "User registered successfully.";
        }
    }
}
