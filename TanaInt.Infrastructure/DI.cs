using Microsoft.Extensions.DependencyInjection;
using TanaInt.Infrastructure.Services;

namespace TanaInt.Infrastructure;

public static class DI
{
    public static IServiceCollection AddGCalServices(this IServiceCollection services)
    {
        services.AddSingleton<IGCalService, GCalService>();
        return services;
    }
    public static IServiceCollection AddBannerChangerService(this IServiceCollection services)
    {
        services.AddSingleton<IBannerChangerService, BannerChangerService>();
        return services;
    }
    public static IServiceCollection AddCalendarRecurrenceServices(this IServiceCollection services)
    {
        services.AddSingleton<ICalendarRecurrenceService, CalendarRecurrenceService>();
        return services;
    }

    public static IServiceCollection AddFsrsServices(this IServiceCollection services)
    {
        services.AddScoped<IFsrsService, FsrsService>();
        services.AddScoped<IRequestTimeZoneProvider, RequestTimeZoneProvider>();
        return services;
    }
}