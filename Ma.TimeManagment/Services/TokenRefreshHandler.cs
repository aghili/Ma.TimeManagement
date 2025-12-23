using System.Net.Http;

namespace Ma.TimeManagement.Services
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;
     
        public TokenRefreshHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request.RequestUri.LocalPath.Contains("Auth"))
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            // Get the current token (proactive)
            var token = await _tokenService.GetTokenAsync(false,cancellationToken);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            // If expired (reactive)
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _tokenService.InvalidateToken();
                var newToken = await _tokenService.GetTokenAsync(forceRefresh: true,cancellationToken);

                // Re-send with new token
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                return await base.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}