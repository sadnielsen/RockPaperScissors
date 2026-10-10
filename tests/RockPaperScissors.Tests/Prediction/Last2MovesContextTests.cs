using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Prediction;

public sealed class Last2MovesContextTests
{
    private readonly Last2MovesContext _context = new();

    [Fact]
    public void GetKey_EmptyHistory_ReturnsNull()
    {
        Assert.Null(_context.GetKey(TestHistory.Empty));
    }

    [Fact]
    public void GetKey_OneRound_ReturnsNull()
    {
        Assert.Null(_context.GetKey(TestHistory.Of((Move.Rock, Move.Paper))));
    }

    [Fact]
    public void GetKey_SeveralRounds_ReturnsLastTwoPlayerMovesInOrder()
    {
        var history = TestHistory.Of((Move.Rock, Move.Rock), (Move.Paper, Move.Rock), (Move.Scissors, Move.Rock));

        Assert.Equal(new Last2MovesKey(Move.Paper, Move.Scissors), _context.GetKey(history));
    }

    [Fact]
    public void GetKey_SameMovesInOtherOrder_ReturnsDifferentKeys()
    {
        var rockThenPaper = TestHistory.Of((Move.Rock, Move.Rock), (Move.Paper, Move.Rock));
        var paperThenRock = TestHistory.Of((Move.Paper, Move.Rock), (Move.Rock, Move.Rock));

        Assert.NotEqual(_context.GetKey(rockThenPaper), _context.GetKey(paperThenRock));
    }
}
