using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Playwrights.Installation.Abstract;
using Soenneker.Utils.Directory.Registrars;

namespace Soenneker.Playwrights.Installation.Registrars;

/// <summary>
/// A utility library for Playwright installation assurance
/// </summary>
public static class PlaywrightInstallationUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a singleton.
    /// </summary>
    public static IServiceCollection AddPlaywrightInstallationUtilAsSingleton(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsSingleton()
                .TryAddSingleton<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IPlaywrightInstallationUtil"/> as a scoped service.
    /// </summary>
    public static IServiceCollection AddPlaywrightInstallationUtilAsScoped(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsScoped()
                .TryAddScoped<IPlaywrightInstallationUtil, PlaywrightInstallationUtil>();

        return services;
    }
}