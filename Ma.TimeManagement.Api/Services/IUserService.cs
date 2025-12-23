
using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface IUserService
    {
        Task<User?> ValidateUser(Guid Username,CancellationToken cancellationToken);
        Task SavePatAsync(Guid userId, string plainPat,CancellationToken cancellationToken);
        Task<string> GetPatAsync(Guid userId,CancellationToken cancellationToken);
        Task<User?> GetUserAsync(string username,CancellationToken cancellationToken);
        Task<User> CreateUserAsync(string username, string pAT,CancellationToken cancellationToken);
        Task<string> GetCurrentUserPatAsync(CancellationToken cancellationToken);
        Task SavePatForCurrentUserAsync(string plainPat, CancellationToken cancellationToken);
        Task<bool> IsUserExistAsync(string username, CancellationToken cancellationToken);
    }
}
