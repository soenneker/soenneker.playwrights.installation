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
    /// <returns>The result of the operation.</returns>
    string GetPlaywrightPath();

    /// <summary>
    /// Sets options for installation. Call before <see cref="EnsureInstalled"/>; has no effect after the first install.
    /// </summary>
    void SetOptions(PlaywrightInstallationOptions options);

    /// <summary>
    /// Executes the ensure installed operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask EnsureInstalled(CancellationToken cancellationToken = default);
}
