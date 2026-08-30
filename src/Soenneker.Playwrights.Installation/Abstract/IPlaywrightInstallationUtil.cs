using System;
using System.Threading;
using System.Threading.Tasks;

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
    /// Sets options for installation. This must be called before <see cref="EnsureInstalled"/> begins.
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
}
