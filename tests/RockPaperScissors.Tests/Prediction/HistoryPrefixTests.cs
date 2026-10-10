using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Prediction;

public sealed class HistoryPrefixTests
{
    private static readonly IReadOnlyList<Round> History =
        TestHistory.Of((Move.Rock, Move.Rock), (Move.Paper, Move.Rock), (Move.Scissors, Move.Rock));

    [Fact]
    public void Enumerate_PrefixOfTwo_ReturnsFirstTwoRounds()
    {
        var prefix = new HistoryPrefix(History, 2);

        Assert.Equal(2, prefix.Count);
        Assert.Equal(History.Take(2), prefix);
        Assert.Equal(History[1], prefix[1]);
    }

    [Fact]
    public void Indexer_BeyondPrefix_ThrowsArgumentOutOfRangeException()
    {
        var prefix = new HistoryPrefix(History, 2);

        Assert.Throws<ArgumentOutOfRangeException>(() => prefix[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void Constructor_CountOutsideSource_ThrowsArgumentOutOfRangeException(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HistoryPrefix(History, count));
    }
}
