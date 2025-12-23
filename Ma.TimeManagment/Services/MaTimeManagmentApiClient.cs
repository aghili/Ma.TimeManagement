using Ma.TimeManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Ma.TimeManagement.OpenAPIService
{
    public partial class MaTimeManagmentApiClient
    {
        private readonly ISettingsService settingsService;
        private readonly ITokenService tokenService;

        [ActivatorUtilitiesConstructor]
        public MaTimeManagmentApiClient(HttpClient httpClient,ISettingsService settingsService,ITokenService tokenService)
            :this("",httpClient)
        {
            _httpClient = httpClient;
            this.settingsService = settingsService;
            this.tokenService = tokenService;
            BaseUrl = settingsService.FirstServer.ServerUrl;
        }
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            
            //client ??= new HttpClient();

            //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService.GetToken(false));
        }
    }
}
