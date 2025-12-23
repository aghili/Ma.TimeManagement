using Ma.TimeManagement.Models;
using Ma.TimeManagement.OpenAPIService;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net.Http;

namespace Ma.TimeManagement.Services
{
    public class TokenService : ITokenService
    {
        private string? _cachedToken;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService;

        public TokenService(HttpClient httpClient, IServiceProvider serviceProvider, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider; // Inject provider instead of authClient
            _settingsService = settingsService;
        }


        public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync();
            try
            {
                var authClient = _serviceProvider.GetRequiredService<MaTimeManagmentApiClient>();

                if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken))
                    return _cachedToken;
                Guid userID = Guid.Empty;

                RegisterModel registerModel = new() { Username = Path.Combine(Environment.UserDomainName, Environment.UserName), PAT = _settingsService.FirstServer.PAT };

                var regResponse = await authClient.RegisterAsync(registerModel, cancellationToken);
                userID = regResponse.UserID;

                var loginResponse = await authClient.LoginAsync(new LoginModel { UserID = userID }, cancellationToken);
                _cachedToken = loginResponse.Token;

                return _cachedToken!;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally { _lock.Release(); }
        }

        public void InvalidateToken() => _cachedToken = null;

        public string GetToken(bool forceRefresh)
        {
            _lock.Wait();
            try
            {
                var authClient = _serviceProvider.GetRequiredService<MaTimeManagmentApiClient>();

                if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken))
                    return _cachedToken;
                Guid userID = Guid.Empty;
                // 1. Determine UserID (Registry)
                if (string.IsNullOrEmpty(_settingsService.FirstServer.PAT) || !Guid.TryParse(_settingsService.FirstServer.PAT, out userID))
                {
                    RegisterModel registerModel = new() { Username = Environment.UserDomainName };
                    // Using your generated client method
                    var regResponse = authClient.RegisterAsync(registerModel).GetAwaiter().GetResult();
                    userID = regResponse.UserID;
                    _settingsService.FirstServer.PAT = userID.ToString();
                }

                // 2. Get Token (Login)
                // Using your generated client method
                var loginResponse = authClient.LoginAsync(new LoginModel { UserID = userID }).GetAwaiter().GetResult();
                _cachedToken = loginResponse.Token;

                return _cachedToken!;
            }
            finally { _lock.Release(); }
        }
    }
}