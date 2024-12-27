namespace LoginService.Services
{
    public interface IAuthService
    {
        string Authenticate(string username, string password);
        bool ValidateSession(string token);
        void Logout(string token);
    }

}
