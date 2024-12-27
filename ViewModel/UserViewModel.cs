namespace LoginService.ViewModel
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
