using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class RoundTests
{
    [Fact]
    public void Outcome_AnyRound_MatchesRulesResolve()
    {
        foreach (Move player in Enum.GetValues<Move>())
        {
            foreach (Move computer in Enum.GetValues<Move>())
            {
                Assert.Equal(Rules.Resolve(player, computer), new Round(player, computer).Outcome);
            }
        }
    }

    [Fact]
    public void Constructor_WithoutPredictions_HasEmptyPredictions()
    {
        Assert.Empty(new Round(Move.Rock, Move.Paper).Predictions);
    }

    [Fact]
    public void Equals_SameMovesAndEqualPredictionsInOtherLists_IsEqual()
    {
        var a = new Round(Move.Rock, Move.Paper, new List<Move?> { Move.Scissors, null });
        var b = new Round(Move.Rock, Move.Paper, new[] { (Move?)Move.Scissors, null });

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentPredictions_IsNotEqual()
    {
        var a = new Round(Move.Rock, Move.Paper, [Move.Scissors]);
        var b = new Round(Move.Rock, Move.Paper, [Move.Rock]);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_DifferentMoves_IsNotEqual()
    {
        Assert.NotEqual(new Round(Move.Rock, Move.Paper), new Round(Move.Rock, Move.Scissors));
    }
}
