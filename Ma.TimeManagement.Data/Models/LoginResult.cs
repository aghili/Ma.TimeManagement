namespace Ma.TimeManagement.Models
{
    public class LoginResult
    {
        public string Token { get; set; }
        public bool PatConfigured { get; set; }
    }
    public class RegisterResult
    {
        public Guid UserID { get; set; }
    }
}