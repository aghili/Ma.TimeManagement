using Ma.TimeManagement.Data;
using Ma.TimeManagement.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ma.TimeManagement.Api.Extentions
{
    public static class IserviceCollectionExtension 
    {
        public static IServiceCollection AddLocalServices(this IServiceCollection services,WebApplicationBuilder hostBuilder)
        {
            services.AddSingleton<IStaticDataService, StaticDataService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IPatEncryption, PatEncryptionNone>();
            services.AddScoped<ICurrentUserPatService, CurrentUserPatService>();
            services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(hostBuilder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Ma.TimeManagement.Migrations.SqlServer")));
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IStatusService, StatusServiceConsole>();
            services.AddScoped<IConverterService, ConverterService>();
            services.AddScoped<IAzureDevOpsService, AzureDevOpsService>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}
