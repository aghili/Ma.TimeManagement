using Ma.TimeManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Ma.TimeManagement.OpenAPIService
{
    public partial class MaTimeManagementApiClient
    {
        private readonly ISettingsService settingsService;
        private readonly ITokenService tokenService;

        [ActivatorUtilitiesConstructor]
        public MaTimeManagementApiClient(HttpClient httpClient,ISettingsService settingsService,ITokenService tokenService)
            :this("",httpClient)
        {
            _httpClient = httpClient;
            this.settingsService = settingsService;
            this.tokenService = tokenService;
            BaseUrl = "https://feed-srv.mhd.mahaksoft.com:1443";//settingsService.FirstServer.ServerUrl;
        }
    }
}
