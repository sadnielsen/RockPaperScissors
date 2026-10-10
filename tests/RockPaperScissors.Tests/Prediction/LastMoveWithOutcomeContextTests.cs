using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Prediction;

public sealed class LastMoveWithOutcomeContextTests
{
    private readonly LastMoveWithOutcomeContext _context = new();

    [Fact]
    public void GetKey_EmptyHistory_ReturnsNull()
    {
        Assert.Null(_context.GetKey(TestHistory.Empty));
    }

    [Fact]
    public void GetKey_SeveralRounds_ReturnsLastPlayerMoveAndOutcome()
    {
        var history = TestHistory.Of((Move.Rock, Move.Rock), (Move.Paper, Move.Scissors));

        Assert.Equal(new LastMoveWithOutcomeKey(Move.Paper, Outcome.Lose), _context.GetKey(history));
    }

    [Fact]
    public void GetKey_SameMoveDifferentOutcome_ReturnsDifferentKeys()
    {
        var won = TestHistory.Of((Move.Rock, Move.Scissors));
        var lost = TestHistory.Of((Move.Rock, Move.Paper));

        Assert.NotEqual(_context.GetKey(won), _context.GetKey(lost));
    }
}
