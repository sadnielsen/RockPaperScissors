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
    public void Resolve_EveryCombination_ReturnsOutcomeFromMatrix(Move player, Move computer, Outcome expected)
    {
        Assert.Equal(expected, Rules.Resolve(player, computer));
    }

    [Theory]
    [InlineData(Move.Rock, Move.Paper)]
    [InlineData(Move.Paper, Move.Scissors)]
    [InlineData(Move.Scissors, Move.Rock)]
    public void CountersOf_EachMove_ReturnsTheMoveThatBeatsIt(Move move, Move expectedCounter)
    {
        Assert.Equal([expectedCounter], Rules.CountersOf(move));
    }

    [Fact]
    public void CountersOf_AnyMove_EveryCounterWinsAgainstIt()
    {
        foreach (Move move in Enum.GetValues<Move>())
        {
            Assert.All(Rules.CountersOf(move), counter => Assert.Equal(Outcome.Win, Rules.Resolve(counter, move)));
        }
    }

    // Guards the matrix itself: in a fair intransitive game every move beats
    // exactly as many moves as it loses to. Stays valid when moves are added.
    [Fact]
    public void Resolve_AllMoves_EveryMoveWinsAsOftenAsItLoses()
    {
        Move[] moves = Enum.GetValues<Move>();

        foreach (Move move in moves)
        {
            int wins = moves.Count(other => Rules.Resolve(move, other) == Outcome.Win);
            int losses = moves.Count(other => Rules.Resolve(move, other) == Outcome.Lose);

            Assert.Equal(wins, losses);
        }
    }

    [Fact]
    public void Resolve_SwappedMoves_GivesOppositeOutcome()
    {
        Move[] moves = Enum.GetValues<Move>();

        foreach (Move a in moves)
        {
            foreach (Move b in moves.Where(b => b != a))
            {
                Assert.NotEqual(Rules.Resolve(a, b), Rules.Resolve(b, a));
            }
        }
    }
}
