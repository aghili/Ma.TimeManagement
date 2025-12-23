using Ma.TimeManagement.Data;
using Ma.TimeManagement.Exceptions;
using Ma.TimeManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading;

namespace Ma.TimeManagement.Services
{
    public class UserService : IUserService
    {
        public UserService(IPatEncryption patEncryption, IDbContextFactory<ApplicationDbContext> dbContextFactory, IHttpContextAccessor httpContextAccessor)
        {
            this.patEncryption = patEncryption;
            this.dbContextFactory = dbContextFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        private readonly IPatEncryption patEncryption;
        private readonly IDbContextFactory<ApplicationDbContext> dbContextFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        public async Task<string> GetCurrentUserPatAsync(CancellationToken cancellationToken)
        {
            var user = await GetOrCreateCurrentUserAsync(cancellationToken);

            if (string.IsNullOrEmpty(user.AdoPatEncrypted))
            {
                throw new UnauthorizedAccessException(
                    "Azure DevOps PAT not configured. Please call POST /api/profile/save-pat to set it up.");
            }

            var decryptedPat = patEncryption.Decrypt(user.AdoPatEncrypted);
            if (string.IsNullOrEmpty(decryptedPat))
            {
                throw new UnauthorizedAccessException("Failed to decrypt PAT. It may be corrupted.");
            }

            return decryptedPat;
        }

        /// <summary>
        /// Gets or creates the current user based on authentication type (Windows or JWT)
        /// </summary>
        private async Task<User> GetOrCreateCurrentUserAsync(CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HttpContext is not available");

            if (!httpContext.User.Identity?.IsAuthenticated ?? false)
                throw new UnauthorizedAccessException("User is not authenticated");

            var claims = httpContext.User;

            // CASE 1: Windows Authentication (Negotiate)
            if (httpContext.User.Identity.AuthenticationType == "Negotiate")
            {
                //var windowsSid = claims.FindFirst(ClaimTypes.PrimarySid)?.Value
                //              ?? claims.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid")?.Value;

                var windowsName = claims.FindFirst(ClaimTypes.Name)?.Value; // e.g. "CONTOSO\john.doe"

                var user = await _db.Users.FirstOrDefaultAsync(u =>
                    //u.WindowsSid == windowsSid ||
                    u.Username == windowsName);

                if (user != null)
                    return user;

                // Auto-create Windows user on first access
                user = new User
                {
                    Username = windowsName
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return user;
            }

            // CASE 2: JWT Authentication
            if (Guid.TryParse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found in database");

                return user;
            }

            throw new UnauthorizedAccessException("Unable to identify current user");
        }

        // Bonus: Save PAT for current user
        public async Task SavePatForCurrentUserAsync(string plainPat,CancellationToken cancellationToken)
        {
            var user = await GetOrCreateCurrentUserAsync(cancellationToken);
            user.AdoPatEncrypted = patEncryption.Encrypt(plainPat);
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> ValidateUser(Guid userId,CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user;
        }

        public async Task SavePatAsync(Guid userId, string plainPat,CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await _db.Users.FindAsync(userId);
            user.AdoPatEncrypted = patEncryption.Encrypt(plainPat);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> GetPatAsync(Guid userId,CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await _db.Users.FindAsync(userId);
            return patEncryption.Decrypt(user.AdoPatEncrypted);
        }

        public async Task<User?> GetUserAsync(string username,CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await _db.Users.FirstOrDefaultAsync(i => i.Username == username);
            return user;
        }

        public async Task<User> CreateUserAsync(string username, string pAT,CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var exists = await _db.Users.AnyAsync(u => u.Username == username);
            if (exists) throw new EntityExistException("User already exists");

            var user = new User
            {
                Username = username,
                AdoPatEncrypted = patEncryption.Encrypt(pAT)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> IsUserExistAsync(string username, CancellationToken cancellationToken)
        {
            using var _db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await _db.Users.AnyAsync(u => u.Username == username);
        }
    }
}
