using System;
using System.Collections.Generic;
using System.IO;
using Soenneker.Playwrights.Installation.Abstract;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Utils.Runtime;
using Microsoft.Playwright;
using Soenneker.Utils.Directory.Abstract;
using Microsoft.Extensions.Configuration;
using Soenneker.Asyncs.Initializers;
using Soenneker.Asyncs.Locks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Playwrights.Installation.Options;

namespace Soenneker.Playwrights.Installation;

public sealed class PlaywrightInstallationUtil : IPlaywrightInstallationUtil
{
    private readonly ILogger<PlaywrightInstallationUtil> _logger;
    private readonly AsyncInitializer _installer;
    private readonly AsyncLock _optionsLock = new();
    private PlaywrightInstallationOptions? _options;
    private bool? _requiresHeadlessShell;
    private bool _installationStarted;

    public PlaywrightInstallationUtil(ILogger<PlaywrightInstallationUtil> logger, IDirectoryUtil directoryUtil, IConfiguration configuration)
    {
        _logger = logger;

        _installer = new AsyncInitializer(async cancellationToken =>
        {
            PlaywrightInstallationOptions options;
            bool? requiresHeadlessShell;

            using (await _optionsLock.Lock(cancellationToken).NoSync())
            {
                options = _options ?? GetOptions(configuration);
                requiresHeadlessShell = _requiresHeadlessShell;
            }

            logger.LogDebug("Ensuring Playwright {Browser} is installed...", options.Browser);

            string playwrightPath = options.BrowsersPath ?? GetPlaywrightPath();

            await directoryUtil.Create(playwrightPath, false, cancellationToken).NoSync();

            _logger.LogInformation("Setting PLAYWRIGHT_BROWSERS_PATH to {PlaywrightPath}", playwrightPath);

            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", playwrightPath);

            try
            {
                string[] args = BuildInstallArgs(options, requiresHeadlessShell);

                int code = Program.Main(args);

                if (code != 0)
                    throw new Exception($"Playwright CLI exited with {code}");

                logger.LogInformation("Playwright {Browser} installation confirmed.", options.Browser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to install Playwright {Browser}.", options.Browser);
                throw;
            }
        });
    }

    public void SetOptions(PlaywrightInstallationOptions options)
    {
        using (_optionsLock.LockSync())
        {
            if (_installationStarted)
                throw new InvalidOperationException("Playwright installation options cannot be changed after installation has started.");

            _options = options;
        }
    }

    private static PlaywrightInstallationOptions GetOptions(IConfiguration configuration)
    {
        var options = new PlaywrightInstallationOptions();
        configuration.GetSection("Playwright").Bind(options);
        return options;
    }

    private static string[] BuildInstallArgs(PlaywrightInstallationOptions options, bool? requiresHeadlessShell)
    {
        var args = new List<string>(4) { "install" };

        if (options.WithDeps)
            args.Add("--with-deps");

        bool canSkipHeadlessShell = !options.Browser.Equals("chromium", StringComparison.OrdinalIgnoreCase) || requiresHeadlessShell is not true;

        if (options.NoShell && canSkipHeadlessShell)
            args.Add("--no-shell");

        args.Add(options.Browser);

        return [.. args];
    }

    public string GetPlaywrightPath()
    {
        const string playwrightFolder = ".playwright";

        _logger.LogDebug("Resolving Playwright browser path…");

        if (RuntimeUtil.IsAzureAppService)
        {
            const string root = "/home/site/wwwroot";

            _logger.LogInformation("Detected running in Azure App Service");

            return Path.Combine(root, playwrightFolder);
        }

        return Path.Combine(AppContext.BaseDirectory, playwrightFolder);
    }

    public async ValueTask EnsureInstalled(CancellationToken cancellationToken = default)
    {
        using (await _optionsLock.Lock(cancellationToken).NoSync())
        {
            _installationStarted = true;
        }

        await _installer.Init(cancellationToken).NoSync();
    }

    public async ValueTask EnsureInstalled(BrowserTypeLaunchOptions launchOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(launchOptions);

        bool requiresHeadlessShell = launchOptions.Headless is not false && string.IsNullOrWhiteSpace(launchOptions.Channel);

        using (await _optionsLock.Lock(cancellationToken).NoSync())
        {
            if (_installationStarted && _requiresHeadlessShell != requiresHeadlessShell)
                throw new InvalidOperationException("Playwright installation has already started without compatible browser launch options.");

            _requiresHeadlessShell = requiresHeadlessShell;
            _installationStarted = true;
        }

        await _installer.Init(cancellationToken).NoSync();
    }

    public void Dispose()
    {
        _installer.Dispose();
        _optionsLock.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _installer.DisposeAsync().NoSync();
        await _optionsLock.DisposeAsync().NoSync();
    }
}
