[![](https://img.shields.io/nuget/v/soenneker.playwrights.installation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.playwrights.installation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.playwrights.installation/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.playwrights.installation/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.playwrights.installation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.playwrights.installation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.playwrights.installation/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.playwrights.installation/actions/workflows/codeql.yml)

# Soenneker.Playwrights.Installation

Installs a Playwright browser once per application process and configures `PLAYWRIGHT_BROWSERS_PATH` before Playwright starts.

## Related Repos

You might also be interested in:

- [soenneker.playwrights.crawler](https://github.com/soenneker/soenneker.playwrights.crawler) for crawling and mirroring sites to disk with Playwright.
- [soenneker.playwrights.extensions.stealth](https://github.com/soenneker/soenneker.playwrights.extensions.stealth) for stealth-oriented Playwright launch and context extensions.

---

## Quick start

**1. Install the package**

```bash
dotnet add package Soenneker.Playwrights.Installation
```

**2. Register the util** (e.g. in `Program.cs` or your service setup)

```csharp
using Soenneker.Playwrights.Installation.Registrars;

services.AddPlaywrightInstallationUtilAsSingleton();
```

**3. Before creating Playwright, ensure the browser is installed**

```csharp
using Soenneker.Playwrights.Installation.Abstract;

var playwrightUtil = serviceProvider.GetRequiredService<IPlaywrightInstallationUtil>();
await playwrightUtil.EnsureInstalled();

using IPlaywright playwright = await Playwright.CreateAsync();
```

When supplying launch options, pass the same instance to the installer and Playwright. This ensures the required Chromium artifact is installed—for example, default headless mode requires Chromium's separate headless shell:

```csharp
var launchOptions = new BrowserTypeLaunchOptions
{
    Headless = true
};

await playwrightUtil.EnsureInstalled(launchOptions);

using IPlaywright playwright = await Playwright.CreateAsync();
await using IBrowser browser = await playwright.Chromium.LaunchAsync(launchOptions);
```

The first call to `EnsureInstalled()` runs Playwright's installer. Concurrent and later calls share that initialization. Register the utility as a singleton when the application uses one process-wide browser directory.

---

## Changing how it installs (optional)

By default the util installs Chromium with `--no-shell` and `--with-deps`. To change that, call `SetOptions` **before** the first `EnsureInstalled()`:

```csharp
using Soenneker.Playwrights.Installation.Abstract;
using Soenneker.Playwrights.Installation.Options;

playwrightUtil.SetOptions(new PlaywrightInstallationOptions
{
    NoShell = true,           // default: true
    WithDeps = true,          // default: true
    Browser = "chromium",     // or "firefox", "webkit"
    BrowsersPath = null       // optional custom folder for browsers
});

await playwrightUtil.EnsureInstalled();
```

Options are frozen when `EnsureInstalled()` begins; changing them afterward throws `InvalidOperationException`.

- **NoShell** — Passes `--no-shell` to the install command.
- **WithDeps** — Passes `--with-deps` (install system dependencies).
- **Browser** — Which browser to install: `chromium`, `firefox`, or `webkit`.
- **BrowsersPath** — If set, browsers are installed here and `PLAYWRIGHT_BROWSERS_PATH` is set to this path. If `null`, a default path is used (see below).

---

## Where browsers are installed

If you don’t set `BrowsersPath`, the util uses a default directory. You can get that path with:

```csharp
string path = playwrightUtil.GetPlaywrightPath();
```

On Azure App Service it uses a path under the app root. Elsewhere it uses a `.playwright` folder under your app’s base directory.

---

## Using config instead of code (optional)

You can drive the same options from configuration so you don’t have to call `SetOptions` in code. If the **`Playwright`** config section exists, the util uses it when you haven’t called `SetOptions`.

**appsettings.json**

```json
{
  "Playwright": {
    "NoShell": true,
    "WithDeps": true,
    "Browser": "chromium",
    "BrowsersPath": null
  }
}
```

**Environment variables** (use double underscore for the section name)

- `Playwright__NoShell`
- `Playwright__WithDeps`
- `Playwright__Browser`
- `Playwright__BrowsersPath`

Anything you set via `SetOptions` overrides config. If you never call `SetOptions` and there’s no `Playwright` section, the defaults (Chromium, no-shell, with-deps, default path) are used.
