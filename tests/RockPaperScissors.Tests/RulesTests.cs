using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class RulesTests
{
    [Theory]
    [InlineData(Move.Rock, Move.Rock, Outcome.Draw)]
    [InlineData(Move.Rock, Move.Paper, Outcome.Lose)]
    [InlineData(Move.Rock, Move.Scissors, Outcome.Win)]
    [InlineData(Move.Paper, Move.Rock, Outcome.Win)]
    [InlineData(Move.Paper, Move.Paper, Outcome.Draw)]
    [InlineData(Move.Paper, Move.Scissors, Outcome.Lose)]
    [InlineData(Move.Scissors, Move.Rock, Outcome.Lose)]
    [InlineData(Move.Scissors, Move.Paper, Outcome.Win)]
    [InlineData(Move.Scissors, Move.Scissors, Outcome.Draw)]
    public void Resolve_EveryCombination_ReturnsExpectedOutcome(Move player, Move computer, Outcome expected)
    {
        Assert.Equal(expected, Rules.Resolve(player, computer));
    }
}
