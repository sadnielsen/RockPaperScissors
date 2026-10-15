using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class GameSessionTests
{
    [Fact]
    public void IsFinished_AfterExactlyTotalRounds_BecomesTrue()
    {
        var session = new GameSession(3, new FakeRandom(Move.Rock, Move.Paper, Move.Scissors));

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
        var session = new GameSession(1, new FakeRandom(Move.Rock));
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

    [Theory]
    // Player plays Rock; the computer's moves decide the outcome of each round.
    [InlineData(Outcome.Win, Move.Scissors, Move.Scissors, Move.Paper)]
    [InlineData(Outcome.Lose, Move.Paper, Move.Paper, Move.Scissors)]
    [InlineData(Outcome.Draw, Move.Scissors, Move.Paper, Move.Rock)]
    public void Result_FinishedGame_FollowsWinsAndLosses(Outcome expected, params Move[] computerMoves)
    {
        var session = new GameSession(computerMoves.Length, new FakeRandom(computerMoves));

        while (!session.IsFinished)
        {
            session.PlayRound(Move.Rock);
        }

        Assert.Equal(expected, session.Result);
    }

    [Fact]
    public void Result_NoRoundsPlayed_Throws()
    {
        var session = new GameSession(1, new FakeRandom());

        Assert.Throws<InvalidOperationException>(() => session.Result);
    }

    [Fact]
    public void Result_GameNotFinished_Throws()
    {
        var session = new GameSession(2, new FakeRandom(Move.Scissors));
        session.PlayRound(Move.Rock);

        Assert.Throws<InvalidOperationException>(() => session.Result);
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

    // Guards fairness: with the same random numbers, the player's moves must not change the computer's.
    [Fact]
    public void PlayRound_DifferentPlayerMoves_ComputerMovesAreTheSame()
    {
        const int seed = 42;
        Move[] moves = Enum.GetValues<Move>();
        var alwaysRock = new GameSession(30, new Random(seed));
        var cycling = new GameSession(30, new Random(seed));

        for (int round = 0; !alwaysRock.IsFinished; round++)
        {
            alwaysRock.PlayRound(Move.Rock);
            cycling.PlayRound(moves[round % moves.Length]);
        }

        Assert.Equal(
            alwaysRock.History.Select(round => round.Computer),
            cycling.History.Select(round => round.Computer));
    }

    [Theory]
    [InlineData(GameSession.MinRounds, true)]
    [InlineData(GameSession.MaxRounds, true)]
    [InlineData(GameSession.MinRounds - 1, false)]
    [InlineData(-1, false)]
    [InlineData(GameSession.MaxRounds + 1, false)]
    public void IsValidTotalRounds_Boundaries_ReturnsExpected(int totalRounds, bool expected)
    {
        Assert.Equal(expected, GameSession.IsValidTotalRounds(totalRounds));
    }

    [Theory]
    [InlineData(GameSession.MinRounds - 1)]
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
