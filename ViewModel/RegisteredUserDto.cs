﻿namespace LoginService.ViewModel
{
    public class RegisteredUserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

}