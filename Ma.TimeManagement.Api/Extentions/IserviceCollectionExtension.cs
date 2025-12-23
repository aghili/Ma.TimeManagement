using Ma.TimeManagement.Data;
using Ma.TimeManagement.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ma.TimeManagement.Api.Extentions
{
    public static class IserviceCollectionExtension 
    {
        public static IServiceCollection AddLocalServices(this IServiceCollection services)
        {
            var staticDataInstance = new StaticDataService();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IPatEncryption, PatEncryption>();
            services.AddScoped<ICurrentUserPatService, CurrentUserPatService>();
            services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
            var connectionString = new SqliteConnectionStringBuilder() { DataSource = staticDataInstance.PathFullDatabase, Cache = SqliteCacheMode.Shared, Pooling = true }.ConnectionString;
            services.AddSingleton<IStaticDataService>(staticDataInstance);
            services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IStatusService, StatusServiceConsole>();
            services.AddScoped<IConverterService, ConverterService>();
            services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}
