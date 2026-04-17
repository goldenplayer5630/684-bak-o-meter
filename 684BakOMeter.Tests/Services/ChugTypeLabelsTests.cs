using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;

namespace _684BakOMeter.Tests.Services;

public class ChugTypeLabelsTests
{
    [Theory]
    [InlineData(ChugType.Bak, "Bak")]
    [InlineData(ChugType.Pul, "Pul")]
    [InlineData(ChugType.BakPlus, "Bak+")]
    [InlineData(ChugType.IceFles, "Ice Fles")]
    [InlineData(ChugType.SpaRood, "Spa Rood")]
    [InlineData(ChugType.Wijn, "Wijn")]
    [InlineData(ChugType.Pitcher, "Pitcher")]
    public void GetLabel_ReturnsExpectedLabel(ChugType type, string expected)
    {
        Assert.Equal(expected, ChugTypeLabels.GetLabel(type));
    }

    [Fact]
    public void All_ContainsEveryChugType()
    {
        foreach (var type in Enum.GetValues<ChugType>())
            Assert.True(ChugTypeLabels.All.ContainsKey(type), $"Missing label for {type}");
    }

    [Fact]
    public void Main_DoesNotContainHiddenTypes()
    {
        foreach (var hidden in ChugTypeLabels.Hidden.Keys)
            Assert.False(ChugTypeLabels.Main.ContainsKey(hidden), $"{hidden} should not be in Main");
    }

    [Fact]
    public void Hidden_ContainsWijnAndPitcher()
    {
        Assert.Contains(ChugType.Wijn, ChugTypeLabels.Hidden.Keys);
        Assert.Contains(ChugType.Pitcher, ChugTypeLabels.Hidden.Keys);
    }

    [Theory]
    [InlineData(ChugType.Bak)]
    [InlineData(ChugType.Pul)]
    public void GetSlug_ReturnsEnumName(ChugType type)
    {
        Assert.Equal(type.ToString(), ChugTypeLabels.GetSlug(type));
    }
}
