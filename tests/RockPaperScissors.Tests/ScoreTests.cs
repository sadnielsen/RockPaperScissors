using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class ScoreTests
{
    [Theory]
    [InlineData(Outcome.Win, 1, 0, 0)]
    [InlineData(Outcome.Lose, 0, 1, 0)]
    [InlineData(Outcome.Draw, 0, 0, 1)]
    public void Add_Outcome_IncrementsOnlyMatchingCounter(Outcome outcome, int wins, int losses, int draws)
    {
        Assert.Equal(new Score(wins, losses, draws), new Score().Add(outcome));
    }

    [Fact]
    public void Add_UndefinedOutcome_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Score().Add((Outcome)42));
    }
}
