using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<IRepository, Repository>();
        collection.AddTransient<BusinessService>();
        collection.AddTransient<MainViewModel>();
    }
}