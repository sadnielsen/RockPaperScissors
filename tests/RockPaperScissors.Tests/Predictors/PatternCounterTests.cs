using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Predictors;

public sealed class PatternCounterTests
{
    [Fact]
    public void MostFrequentNext_EmptyHistory_ReturnsNull()
    {
        Assert.Null(PatternCounter.MostFrequentNext([], _ => true));
    }

    [Fact]
    public void MostFrequentNext_NoMatchingSituation_ReturnsNull()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Scissors);

        Assert.Null(PatternCounter.MostFrequentNext(history, _ => false));
    }

    [Fact]
    public void MostFrequentNext_SomeSituationsMatch_CountsOnlyMovesAfterMatches()
    {
        // Matching i = 1 and i = 3 counts Paper twice; the Scissors at i = 2 is not counted.
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Scissors, Move.Paper);

        Assert.Equal(Move.Paper, PatternCounter.MostFrequentNext(history, i => i != 2));
    }

    [Fact]
    public void MostFrequentNext_NeverAsksAboutFirstRound()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper);
        var asked = new List<int>();

        PatternCounter.MostFrequentNext(history, i => { asked.Add(i); return true; });

        Assert.Equal([1], asked);
    }

    [Fact]
    public void MostFrequentNext_SharedLead_ReturnsNull()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Scissors);

        Assert.Null(PatternCounter.MostFrequentNext(history, _ => true));
    }

    [Fact]
    public void MostFrequent_NoMoves_ReturnsNull()
    {
        Assert.Null(PatternCounter.MostFrequent([]));
    }

    [Fact]
    public void MostFrequent_ClearLeader_ReturnsLeader()
    {
        Assert.Equal(Move.Scissors, PatternCounter.MostFrequent([Move.Scissors, Move.Rock, Move.Scissors]));
    }

    [Fact]
    public void MostFrequent_SharedLead_ReturnsNull()
    {
        Assert.Null(PatternCounter.MostFrequent([Move.Rock, Move.Paper, Move.Paper, Move.Rock, Move.Scissors]));
    }
}
