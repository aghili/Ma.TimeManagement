using System.Net.Http;

namespace Ma.TimeManagement.Services
{
    //public class WindowsAuthHandler : DelegatingHandler
    //{
    //    private readonly ISettingsService _settings; // Your service to save UserID
    //    private readonly ITokenService tokenService;

    //    // You might need a reference to a 'LoginClient' here to perform the auth calls

    //    public WindowsAuthHandler(ISettingsService settings,ITokenService tokenService)
    //    {
    //        _settings = settings;
    //        this.tokenService = tokenService;
    //    }

    //    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //    {
    //        // 3. Get the Token using the UserID
    //        var token = await GetTokenByUserIdAsync();

    //        // 4. Attach the token to the header
    //        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    //        return await base.SendAsync(request, cancellationToken);
    //    }

    //    private async Task<string> RegisterUserIDAsync()
    //    {
    //        //string windowsUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
    //        // Call your Register endpoint here...
    //        return tokenService.;
    //    }

    //    private async Task<string> GetTokenByUserIdAsync(string userId)
    //    {
    //        // Call your Login endpoint here...
    //        return "jwt-token-string";
    //    }
    //}
}