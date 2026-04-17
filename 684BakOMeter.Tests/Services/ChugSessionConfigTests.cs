using _684BakOMeter.Web.Services;

namespace _684BakOMeter.Tests.Services;

public class ChugSessionConfigTests
{
    [Fact]
    public void Defaults_EmptyThresholdIsLowerThanFull()
    {
        var config = new ChugSessionConfig();
        Assert.True(config.EmptyThreshold < config.FullThreshold);
    }

    [Fact]
    public void Defaults_HaveSensibleValues()
    {
        var config = new ChugSessionConfig();
        Assert.Equal(50_000m, config.EmptyThreshold);
        Assert.Equal(70_000m, config.FullThreshold);
        Assert.Equal(10_000m, config.InvalidTolerance);
    }

    [Fact]
    public void CustomValues_AreStored()
    {
        var config = new ChugSessionConfig
        {
            EmptyThreshold = 30_000m,
            FullThreshold = 60_000m,
            InvalidTolerance = 5_000m,
        };

        Assert.Equal(30_000m, config.EmptyThreshold);
        Assert.Equal(60_000m, config.FullThreshold);
        Assert.Equal(5_000m, config.InvalidTolerance);
    }
}
