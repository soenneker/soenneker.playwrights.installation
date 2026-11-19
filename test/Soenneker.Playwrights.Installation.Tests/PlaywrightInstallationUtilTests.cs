using Soenneker.Playwrights.Installation.Abstract;
using Soenneker.Playwrights.Installation.Options;
using Soenneker.Tests.HostedUnit;
using System;
using System.Threading.Tasks;

namespace Soenneker.Playwrights.Installation.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class PlaywrightInstallationUtilTests : HostedUnitTest
{
    private readonly IPlaywrightInstallationUtil _util;

    public PlaywrightInstallationUtilTests(Host host) : base(host)
    {
        _util = Resolve<IPlaywrightInstallationUtil>(true);
    }

    [Test]
    public async Task FailedInstallationIncludesCliDiagnostic()
    {
        _util.SetOptions(new PlaywrightInstallationOptions { Browser = "invalid-test-browser", WithDeps = false });
        try
        {
            await _util.EnsureInstalled();
        }
        catch (InvalidOperationException ex)
        {
            await Assert.That(ex.Message).Contains("exited with code");
            await Assert.That(ex.Message).Contains("Invalid installation targets");
            return;
        }

        throw new InvalidOperationException("Expected Playwright to reject the invalid browser.");
    }
}
