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
        Assert.Equal(67_000m, config.EmptyContainerWeight);
        Assert.Equal(82_000m, config.FullContainerWeight);
    }

    [Fact]
    public void CustomValues_AreStored()
    {
        var config = new ChugSessionConfig
        {
            EmptyThreshold = 30_000m,
            FullThreshold = 60_000m,
            EmptyContainerWeight = 55_000m,
            FullContainerWeight = 75_000m,
        };

        Assert.Equal(30_000m, config.EmptyThreshold);
        Assert.Equal(60_000m, config.FullThreshold);
        Assert.Equal(55_000m, config.EmptyContainerWeight);
        Assert.Equal(75_000m, config.FullContainerWeight);
    }

    [Fact]
    public void ValidationDelaySeconds_Is5()
    {
        Assert.Equal(5, ChugSessionConfig.ValidationDelaySeconds);
    }
}
