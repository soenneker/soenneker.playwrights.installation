using Soenneker.Playwrights.Installation.Abstract;
using Soenneker.Tests.HostedUnit;

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
    public void Default()
    {

    }
}
