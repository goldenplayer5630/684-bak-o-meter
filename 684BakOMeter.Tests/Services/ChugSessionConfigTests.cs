using _684BakOMeter.Web.Services;

namespace _684BakOMeter.Tests.Services;

public class ChugSessionConfigTests
{
    [Fact]
    public void Defaults_HaveSensibleValues()
    {
        var config = new ChugSessionConfig();
        Assert.True(config.LiftDropFactor > 0);
        Assert.True(config.LiftDropFactor < 1);
        Assert.True(config.ReturnConfirmReadings > 0);
    }

    [Fact]
    public void CustomValues_AreStored()
    {
        var config = new ChugSessionConfig
        {
            LiftDropFactor        = 0.4m,
            ReturnConfirmReadings = 4,
        };

        Assert.Equal(0.4m, config.LiftDropFactor);
        Assert.Equal(4,    config.ReturnConfirmReadings);
    }
}
