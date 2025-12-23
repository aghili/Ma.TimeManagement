using System.Runtime.Serialization;

namespace Ma.TimeManagement.Models
{
    [DataContract]
    public class RegisterModel
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string PAT { get; set; }
    }
}