using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class GameSessionTests
{
    [Fact]
    public void IsFinished_AfterExactlyTotalRounds_BecomesTrue()
    {
        var session = new GameSession(3, new FakeRandom());

        for (int round = 0; round < 3; round++)
        {
            Assert.False(session.IsFinished);
            session.PlayRound(Move.Rock);
        }

        Assert.True(session.IsFinished);
    }

    [Fact]
    public void PlayRound_AfterLastRound_Throws()
    {
        var session = new GameSession(1, new FakeRandom());
        session.PlayRound(Move.Rock);

        Assert.Throws<InvalidOperationException>(() => session.PlayRound(Move.Rock));
        Assert.Single(session.History);
    }

    [Fact]
    public void PlayRound_AnyMove_ReturnsRoundWithBothMovesAndAddsItToHistory()
    {
        var session = new GameSession(1, new FakeRandom(Move.Scissors));

        Round round = session.PlayRound(Move.Rock);

        Assert.Equal(new Round(Move.Rock, Move.Scissors), round);
        Assert.Equal(Outcome.Win, round.Outcome);
        Assert.Equal([round], session.History);
    }

    [Fact]
    public void Score_MixedOutcomes_MatchesHistory()
    {
        // Computer plays Scissors, Paper, Rock, Scissors against four Rocks: win, lose, draw, win.
        var session = new GameSession(4, new FakeRandom(Move.Scissors, Move.Paper, Move.Rock, Move.Scissors));

        for (int round = 0; round < 4; round++)
        {
            session.PlayRound(Move.Rock);
        }

        Assert.Equal(2, session.Wins);
        Assert.Equal(1, session.Losses);
        Assert.Equal(1, session.Draws);
    }

    [Fact]
    public void PlayRound_SeededRandom_ComputerPlaysEveryMove()
    {
        var session = new GameSession(100, new Random(42));

        while (!session.IsFinished)
        {
            session.PlayRound(Move.Rock);
        }

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), session.History.Select(round => round.Computer).ToHashSet());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(GameSession.MaxRounds + 1)]
    public void Constructor_InvalidTotalRounds_Throws(int totalRounds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSession(totalRounds, new FakeRandom()));
    }

    [Fact]
    public void Constructor_MaxRounds_IsAllowed()
    {
        Assert.Equal(GameSession.MaxRounds, new GameSession(GameSession.MaxRounds, new FakeRandom()).TotalRounds);
    }
}
