using RockPaperScissors.Console;

namespace RockPaperScissors.Tests.Console;

public sealed class RoundCountParserTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("1000", 1000)]
    [InlineData(" 5 ", 5)]
    public void TryParse_NumberInRange_ReturnsRounds(string input, int expected)
    {
        Assert.True(RoundCountParser.TryParse(input, out var rounds));
        Assert.Equal(expected, rounds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1001")]
    [InlineData("2.5")]
    [InlineData("99999999999")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(RoundCountParser.TryParse(input, out var rounds));
        Assert.Equal(0, rounds);
    }
}
