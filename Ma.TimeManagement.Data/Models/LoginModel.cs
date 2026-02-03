namespace Ma.TimeManagement.Models
{
    public class LoginModel
    {
        public Guid UserID { get; set; }
    }

    public class MessageModel
    {
        public double Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public Guid SenderUserID { get; set; }
        public Guid ResiverUserID { get; set; }

        public virtual User Sender {  get; set; }
        public virtual User Resiver { get; set; }
    }
}