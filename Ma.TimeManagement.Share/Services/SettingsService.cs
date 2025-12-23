using Ma.TimeManagement.Models;
using System.IO;

namespace Ma.TimeManagement.Services
{
    public class SettingsService : ISettingsService
    {
        private SettingGeneralModel _general;
        private readonly IStaticDataService staticDataService;

        private SettingGeneralModel General => _general ??= (GetSetting<SettingGeneralModel>() ?? new SettingGeneralModel());

        private T? GetSetting<T>()
        {
            string appsettings_path = staticDataService.PathFullSettings;

            if (!File.Exists(appsettings_path)) return default;
            string json = File.ReadAllText(appsettings_path);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }

        public SettingsService(IStaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }

        public List<AzureServerItemModel> Servers
        {
            get
            {
                return General.Servers;
            }
            set
            {
                General.Servers = value;
                AddUpdateAppSettings(General);
            }
        }

        public AzureServerItemModel FirstServer
        {
            set
            {
                General.Servers.Clear();
                General.Servers.Add(value);
                AddUpdateAppSettings(General);
            }

            get => General.Servers.First();
        }

        public bool BypassProxyOnLocal
        {
            set
            {
                General.BypassProxyOnLocal = value;
                AddUpdateAppSettings(General);
            }

            get => General.BypassProxyOnLocal;
        }

        private Task AddUpdateAppSettings(SettingGeneralModel general)
        {
            string appsettings_path = staticDataService.PathFullSettings;

            return File.WriteAllTextAsync(appsettings_path, System.Text.Json.JsonSerializer.Serialize(general, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
