namespace Ma.TimeManagement.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
     
        // Encrypted Azure DevOps PAT
        public string? AdoPatEncrypted { get; set; }
        public string? AdoPatIv { get; set; }        // optional – if you later switch to AES-GCM
    }
}