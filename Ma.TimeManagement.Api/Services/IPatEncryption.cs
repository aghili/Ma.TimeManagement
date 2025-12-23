namespace Ma.TimeManagement.Services
{
    public interface IPatEncryption
    {
        string Decrypt(string cipherText);
        string Encrypt(string plainText);
    }
}