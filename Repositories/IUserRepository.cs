using LoginService.Models;

namespace LoginService.Repositories
{
    public interface IUserRepository
    {
        User GetUserByUsername(string username);
        void AddSession(Session session);
        Session GetSessionByToken(string token);
        void InvalidateSession(string token);
        void AddUser(User user);
        User GetUserByEmail(string email);
    }


}
