using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class HistoryPrefixTests
{
    private static readonly IReadOnlyList<Round> Source =
        HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Scissors, Move.Rock);

    [Fact]
    public void Count_PrefixOfTwo_ReturnsTwo()
    {
        Assert.Equal(2, new HistoryPrefix(Source, 2).Count);
    }

    [Fact]
    public void Enumerate_PrefixOfTwo_YieldsOnlyFirstTwoRounds()
    {
        Assert.Equal(Source.Take(2), new HistoryPrefix(Source, 2));
    }

    [Fact]
    public void Indexer_WithinPrefix_ReturnsSourceRound()
    {
        Assert.Same(Source[1], new HistoryPrefix(Source, 2)[1]);
    }

    [Fact]
    public void Indexer_PastPrefixButWithinSource_Throws()
    {
        var prefix = new HistoryPrefix(Source, 2);

        Assert.Throws<ArgumentOutOfRangeException>(() => prefix[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void Constructor_CountOutsideSource_Throws(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HistoryPrefix(Source, count));
    }
}
