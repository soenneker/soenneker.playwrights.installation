using Soenneker.Playwrights.Installation.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Playwrights.Installation.Tests;

[Collection("Collection")]
public sealed class PlaywrightInstallationUtilTests : FixturedUnitTest
{
    private readonly IPlaywrightInstallationUtil _util;

    public PlaywrightInstallationUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IPlaywrightInstallationUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
