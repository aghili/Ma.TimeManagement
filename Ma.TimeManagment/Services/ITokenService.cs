
namespace Ma.TimeManagement.Services
{
    public interface ITokenService
    {
        string GetToken(bool forceRefresh);
        Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken);
        void InvalidateToken();
    }
}