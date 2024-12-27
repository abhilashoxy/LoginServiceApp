using LoginService.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LoginAppContext _context;

        public UserRepository(LoginAppContext context)
        {
            _context = context;
        }

        public User GetUserByUsername(string username)
        {
            
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public void AddSession(Session session)
        {
            _context.Sessions.Add(session);
            _context.SaveChanges();
        }

        public Session GetSessionByToken(string token)
        {
            return _context.Sessions.FirstOrDefault(s => s.Token == token && s.IsValid);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }


        public void InvalidateSession(string token)
        {
            var session = GetSessionByToken(token);
            if (session != null)
            {
                session.IsValid = false;
                _context.SaveChanges();
            }
        }
        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
    }


}
