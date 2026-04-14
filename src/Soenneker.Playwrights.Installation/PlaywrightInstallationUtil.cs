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
using Soenneker.Extensions.ValueTask;
using Soenneker.Playwrights.Installation.Options;

namespace Soenneker.Playwrights.Installation;

/// <inheritdoc cref="IPlaywrightInstallationUtil"/>
public sealed class PlaywrightInstallationUtil : IPlaywrightInstallationUtil
{
    private readonly ILogger<PlaywrightInstallationUtil> _logger;
    private readonly AsyncInitializer _installer;
    private PlaywrightInstallationOptions? _options;

    public PlaywrightInstallationUtil(ILogger<PlaywrightInstallationUtil> logger, IDirectoryUtil directoryUtil, IConfiguration configuration)
    {
        _logger = logger;

        _installer = new AsyncInitializer(async cancellationToken =>
        {
            PlaywrightInstallationOptions options = _options ?? GetOptions(configuration);

            logger.LogDebug("Ensuring Playwright {Browser} is installed...", options.Browser);

            string playwrightPath = options.BrowsersPath ?? GetPlaywrightPath();

            await directoryUtil.Create(playwrightPath, false, cancellationToken).NoSync();

            _logger.LogInformation("Setting PLAYWRIGHT_BROWSERS_PATH to {PlaywrightPath}", playwrightPath);

            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", playwrightPath);

            try
            {
                string[] args = BuildInstallArgs(options);

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
        _options = options;
    }

    private static PlaywrightInstallationOptions GetOptions(IConfiguration configuration)
    {
        var options = new PlaywrightInstallationOptions();
        configuration.GetSection("Playwright").Bind(options);
        return options;
    }

    private static string[] BuildInstallArgs(PlaywrightInstallationOptions options)
    {
        var args = new List<string>(4) { "install" };

        if (options.WithDeps)
            args.Add("--with-deps");

        if (options.NoShell)
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

    public ValueTask EnsureInstalled(CancellationToken cancellationToken = default)
    {
        return _installer.Init(cancellationToken);
    }

    public void Dispose()
    {
        _installer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _installer.DisposeAsync();
    }
}
