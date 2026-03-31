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
    string GetPlaywrightPath();

    /// <summary>
    /// Sets options for installation. Call before <see cref="EnsureInstalled"/>; has no effect after the first install.
    /// </summary>
    void SetOptions(PlaywrightInstallationOptions options);

    ValueTask EnsureInstalled(CancellationToken cancellationToken = default);
}
