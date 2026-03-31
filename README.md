[![](https://img.shields.io/nuget/v/soenneker.playwrights.installation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.playwrights.installation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.playwrights.installation/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.playwrights.installation/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.playwrights.installation.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.playwrights.installation/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.playwrights.installation/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.playwrights.installation/actions/workflows/codeql.yml)

# Soenneker.Playwrights.Installation

Makes sure Playwright’s browser (e.g. Chromium) is installed before you use it. It runs the install once, sets the browser path, and you’re done.

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

**3. Before using Playwright, ensure it’s installed**

```csharp
using Soenneker.Playwrights.Installation.Abstract;

var playwrightUtil = serviceProvider.GetRequiredService<IPlaywrightInstallationUtil>();
await playwrightUtil.EnsureInstalled();
// Now use Playwright as usual.
```

The first call to `EnsureInstalled()` installs the browser if needed. Later calls do nothing. You only need to call it once per process.

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
