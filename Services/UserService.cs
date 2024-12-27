using LoginService.Models;
using LoginService.Repositories;
using LoginService.ViewModel;

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
        public async Task<List<RegisteredUserDto>> GetRegisteredUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            // Transform the data into DTOs
            return users.Select(user => new RegisteredUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                CreatedDate = user.CreatedDate
            }).ToList();
        }

    }
}
