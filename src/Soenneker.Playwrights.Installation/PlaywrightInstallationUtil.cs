using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Soenneker.Extensions.Task;
using Soenneker.Playwrights.Installation.Options;

namespace Soenneker.Playwrights.Installation;

/// <inheritdoc cref="IPlaywrightInstallationUtil" />
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

                var cli = new Program();
                var result = cli.RunWithResult(args);
                // Node must start before Playwright can handle --with-deps.
                if (options.WithDeps && OperatingSystem.IsLinux() && result.ExitCode == 127 &&
                    result.StandardError?.Contains("libatomic.so.1: cannot open shared object file", StringComparison.Ordinal) == true &&
                    File.Exists("/usr/bin/apt-get"))
                {
                    logger.LogInformation("Installing libatomic1 for the Playwright driver. Loader diagnostic: {Diagnostic}", OutputTail(result.StandardError));
                    await RunApt(["update"], cancellationToken).NoSync();
                    await RunApt(["install", "-y", "--no-install-recommends", "libatomic1"], cancellationToken).NoSync();
                    cancellationToken.ThrowIfCancellationRequested();
                    result = cli.RunWithResult(args);
                }
                if (result.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Playwright {string.Join(" ", args)} exited with code {result.ExitCode}. " +
                        $"stderr: {OutputTail(result.StandardError)}\nstdout: {OutputTail(result.StandardOutput)}");
                }

                logger.LogInformation("Playwright {Browser} installation confirmed.", options.Browser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to install Playwright {Browser}.", options.Browser);
                throw;
            }
        });
    }

    private static string OutputTail(string? output)
    {
        const int limit = 4096;
        string text = output?.Trim() ?? string.Empty;
        return text.Length > limit ? text[^limit..] : text;
    }

    private static async Task RunApt(string[] arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var start = new ProcessStartInfo("/usr/bin/apt-get")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        if (Environment.UserName != "root" && File.Exists("/usr/bin/sudo"))
        {
            start.FileName = "/usr/bin/sudo";
            start.ArgumentList.Add("-n");
            start.ArgumentList.Add("/usr/bin/apt-get");
        }

        start.Environment["DEBIAN_FRONTEND"] = "noninteractive";
        foreach (string argument in arguments)
            start.ArgumentList.Add(argument);

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("Could not start apt-get to install the Playwright driver dependency libatomic1.");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync(cancellationToken).NoSync();
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            throw;
        }

        string error = await stderr.NoSync();
        string output = await stdout.NoSync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Playwright driver requires libatomic.so.1. apt-get {string.Join(" ", arguments)} exited with code {process.ExitCode}. stderr: {OutputTail(error)}\nstdout: {OutputTail(output)}");
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
