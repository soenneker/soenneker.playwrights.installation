namespace Soenneker.Playwrights.Installation.Options;

/// <summary>
/// Options for Playwright browser installation (e.g. playwright install).
/// </summary>
public sealed class PlaywrightInstallationOptions
{
    /// <summary>
    /// When true, passes --no-shell to the install command. Default is true.
    /// </summary>
    public bool NoShell { get; set; } = true;

    /// <summary>
    /// When true, passes --with-deps to install system dependencies. Default is true.
    /// </summary>
    public bool WithDeps { get; set; } = true;

    /// <summary>
    /// Browser to install (e.g. chromium, firefox, webkit). Default is "chromium".
    /// </summary>
    public string Browser { get; set; } = "chromium";

    /// <summary>
    /// Optional custom path for browser binaries. When set, overrides the default path and PLAYWRIGHT_BROWSERS_PATH.
    /// </summary>
    public string? BrowsersPath { get; set; }
}
