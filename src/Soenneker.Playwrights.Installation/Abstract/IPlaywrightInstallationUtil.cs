using System;
using System.Threading;
using System.Threading.Tasks;

using Soenneker.Playwrights.Installation.Options;

namespace Soenneker.Playwrights.Installation.Abstract;

/// <summary>
/// A utility library for Playwright installation assurance
/// </summary>
public interface IPlaywrightInstallationUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets playwright path.
    /// </summary>
    /// <returns>The requested text.</returns>
    string GetPlaywrightPath();

    /// <summary>
    /// Sets options for installation. Call before <see cref="EnsureInstalled"/>; has no effect after the first install.
    /// </summary>
    /// <param name="options">Options to configure for the Playwright Installation.</param>
    void SetOptions(PlaywrightInstallationOptions options);

    /// <summary>
    /// Ensures installed for the Playwright Installation.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the ensure installed operation is complete.</returns>
    ValueTask EnsureInstalled(CancellationToken cancellationToken = default);
}
