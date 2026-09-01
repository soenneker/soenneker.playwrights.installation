using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

using Soenneker.Playwrights.Installation.Options;

namespace Soenneker.Playwrights.Installation.Abstract;

/// <summary>
/// Ensures that a configured Playwright browser is available before browser automation starts.
/// </summary>
public interface IPlaywrightInstallationUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the default directory used for Playwright browser binaries.
    /// </summary>
    /// <returns>The default browser installation directory for the current host.</returns>
    string GetPlaywrightPath();

    /// <summary>
    /// Sets options for installation. This must be called before <see cref="EnsureInstalled(CancellationToken)"/> begins.
    /// </summary>
    /// <param name="options">Browser, dependency, shell, and installation-path options.</param>
    /// <exception cref="InvalidOperationException">Installation has already started.</exception>
    void SetOptions(PlaywrightInstallationOptions options);

    /// <summary>
    /// Installs the configured browser and its optional system dependencies once per utility instance.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the browser is ready to use.</returns>
    ValueTask EnsureInstalled(CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs the configured browser and any artifacts required by the supplied launch options once per utility instance.
    /// </summary>
    /// <param name="launchOptions">The options that will be passed to Playwright when launching the browser.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the browser is ready to use.</returns>
    /// <exception cref="InvalidOperationException">Installation has already started without compatible launch options.</exception>
    ValueTask EnsureInstalled(BrowserTypeLaunchOptions launchOptions, CancellationToken cancellationToken = default);
}
