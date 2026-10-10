using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class RulesTests
{
    private readonly Rules _rules = new();

    public static TheoryData<Move, Move, Outcome> AllCombinations => new()
    {
        { Move.Rock, Move.Rock, Outcome.Draw },
        { Move.Rock, Move.Paper, Outcome.Lose },
        { Move.Rock, Move.Scissors, Outcome.Win },
        { Move.Paper, Move.Rock, Outcome.Win },
        { Move.Paper, Move.Paper, Outcome.Draw },
        { Move.Paper, Move.Scissors, Outcome.Lose },
        { Move.Scissors, Move.Rock, Outcome.Lose },
        { Move.Scissors, Move.Paper, Outcome.Win },
        { Move.Scissors, Move.Scissors, Outcome.Draw },
    };

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Resolve_AnyCombination_ReturnsOutcomeFromPlayersPerspective(Move player, Move computer, Outcome expected)
    {
        Assert.Equal(expected, _rules.Resolve(player, computer));
    }

    [Theory]
    [InlineData(Move.Rock, Move.Paper)]
    [InlineData(Move.Paper, Move.Scissors)]
    [InlineData(Move.Scissors, Move.Rock)]
    public void CounterOf_Move_ReturnsMoveThatBeatsIt(Move move, Move expectedCounter)
    {
        Assert.Equal(expectedCounter, _rules.CounterOf(move));
    }

    [Theory]
    [InlineData(Move.Rock)]
    [InlineData(Move.Paper)]
    [InlineData(Move.Scissors)]
    public void CounterOf_AnyMove_WinsAgainstThatMove(Move move)
    {
        Assert.Equal(Outcome.Win, _rules.Resolve(_rules.CounterOf(move), move));
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Resolve_SwappedMoves_ReturnsMirroredOutcome(Move player, Move computer, Outcome _)
    {
        var expectedMirror = _rules.Resolve(player, computer) switch
        {
            Outcome.Win => Outcome.Lose,
            Outcome.Lose => Outcome.Win,
            _ => Outcome.Draw,
        };

        Assert.Equal(expectedMirror, _rules.Resolve(computer, player));
    }
}
