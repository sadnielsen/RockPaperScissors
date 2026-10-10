using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class GameSessionTests
{
    private readonly Rules _rules = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(GameSession.MaxRounds + 1)]
    public void Constructor_RoundsOutsideAllowedRange_ThrowsArgumentOutOfRangeException(int totalRounds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GameSession(totalRounds, new SequenceStrategy(Move.Rock), _rules));
    }

    [Theory]
    [InlineData(GameSession.MinRounds)]
    [InlineData(GameSession.MaxRounds)]
    public void Constructor_RoundsAtBoundary_CreatesSession(int totalRounds)
    {
        var session = new GameSession(totalRounds, new SequenceStrategy(Move.Rock), _rules);

        Assert.Equal(totalRounds, session.TotalRounds);
    }

    [Fact]
    public void Constructor_NewSession_ComputerCommitsToFirstMoveBeforeAnyInput()
    {
        var spy = new SpyStrategy();

        _ = new GameSession(3, spy, _rules);

        var observed = Assert.Single(spy.ObservedHistories);
        Assert.Empty(observed);
    }

    [Fact]
    public void PlayRound_LastRound_DoesNotAskStrategyForAnotherMove()
    {
        var spy = new SpyStrategy();
        var session = new GameSession(2, spy, _rules);

        session.PlayRound(Move.Paper);
        session.PlayRound(Move.Paper);

        Assert.Equal(2, spy.ObservedHistories.Count);
    }

    [Fact]
    public void IsFinished_AfterExactlyTotalRounds_BecomesTrue()
    {
        var session = new GameSession(3, new SequenceStrategy(Move.Rock), _rules);

        session.PlayRound(Move.Paper);
        session.PlayRound(Move.Paper);
        Assert.False(session.IsFinished);

        session.PlayRound(Move.Paper);
        Assert.True(session.IsFinished);
        Assert.Equal(3, session.History.Count);
    }

    [Fact]
    public void PlayRound_AfterLastRound_ThrowsInvalidOperationException()
    {
        var session = new GameSession(1, new SequenceStrategy(Move.Rock), _rules);
        session.PlayRound(Move.Paper);

        Assert.Throws<InvalidOperationException>(() => session.PlayRound(Move.Paper));
        Assert.Single(session.History);
    }

    [Fact]
    public void PlayRound_UndefinedMove_ThrowsArgumentOutOfRangeException()
    {
        var session = new GameSession(1, new SequenceStrategy(Move.Rock), _rules);

        Assert.Throws<ArgumentOutOfRangeException>(() => session.PlayRound((Move)42));
        Assert.Empty(session.History);
    }

    [Fact]
    public void PlayRound_KnownComputerMove_ReturnsRoundResolvedByRules()
    {
        var session = new GameSession(1, new SequenceStrategy(Move.Scissors), _rules);

        var round = session.PlayRound(Move.Rock);

        Assert.Equal(new Round(Move.Rock, Move.Scissors, Outcome.Win), round);
        Assert.Equal(round, session.History[0]);
    }

    [Fact]
    public void Score_MixedOutcomes_CountsWinsLossesAndDrawsSeparately()
    {
        // Computer always plays rock: paper wins, scissors loses, rock draws.
        var session = new GameSession(5, new SequenceStrategy(Move.Rock), _rules);

        session.PlayRound(Move.Paper);
        session.PlayRound(Move.Paper);
        session.PlayRound(Move.Scissors);
        session.PlayRound(Move.Rock);
        session.PlayRound(Move.Rock);

        Assert.Equal(new Score(Wins: 2, Losses: 1, Draws: 2), session.Score);
    }

    [Fact]
    public void PlayRound_EachRound_StrategySeesOnlyCompletedRounds()
    {
        var spy = new SpyStrategy();
        var session = new GameSession(3, spy, _rules);

        session.PlayRound(Move.Paper);
        session.PlayRound(Move.Scissors);
        session.PlayRound(Move.Rock);

        // Called exactly once per round, each time with the rounds played before it and nothing more.
        Assert.Equal(3, spy.ObservedHistories.Count);
        for (var round = 0; round < spy.ObservedHistories.Count; round++)
        {
            Assert.Equal(session.History.Take(round), spy.ObservedHistories[round]);
        }
    }

    [Fact]
    public void PlayRound_StrategyReceivesHistory_CannotModifyIt()
    {
        var spy = new SpyStrategy();
        var session = new GameSession(1, spy, _rules);

        session.PlayRound(Move.Paper);

        var asList = Assert.IsAssignableFrom<IList<Round>>(spy.LastHistoryReference);
        Assert.Throws<NotSupportedException>(() => asList.Add(new Round(Move.Rock, Move.Rock, Outcome.Draw)));
    }
}
