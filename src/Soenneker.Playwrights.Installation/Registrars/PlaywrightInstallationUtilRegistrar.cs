using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Playwrights.Installation.Abstract;
using Soenneker.Utils.Directory.Registrars;

namespace Soenneker.Playwrights.Installation.Registrars;

/// <summary>
/// Registers the Playwright installation utility and its dependencies.
/// </summary>
public static class PlaywrightInstallationUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a singleton.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddPlaywrightInstallationUtilAsSingleton(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsSingleton()
                .TryAddSingleton<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a scoped service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddPlaywrightInstallationUtilAsScoped(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsScoped()
                .TryAddScoped<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }
}
