using Ma.TimeManagement.Models;

namespace Ma.TimeManagement.Services
{
    public interface ISettingsService
    {
        AzureServerItemModel FirstServer { get; set; }
        List<AzureServerItemModel> Servers { get; set; }
    }
}