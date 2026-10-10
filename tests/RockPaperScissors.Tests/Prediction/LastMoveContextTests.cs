using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Prediction;

public sealed class LastMoveContextTests
{
    private readonly LastMoveContext _context = new();

    [Fact]
    public void GetKey_EmptyHistory_ReturnsNull()
    {
        Assert.Null(_context.GetKey(TestHistory.Empty));
    }

    [Fact]
    public void GetKey_SeveralRounds_ReturnsLastPlayerMove()
    {
        var history = TestHistory.Of((Move.Rock, Move.Paper), (Move.Scissors, Move.Rock));

        Assert.Equal(new LastMoveKey(Move.Scissors), _context.GetKey(history));
    }

    [Fact]
    public void GetKey_SameLastMoveDifferentComputerMoves_ReturnsEqualKeys()
    {
        var first = TestHistory.Of((Move.Paper, Move.Rock));
        var second = TestHistory.Of((Move.Paper, Move.Scissors));

        Assert.Equal(_context.GetKey(first), _context.GetKey(second));
    }
}
