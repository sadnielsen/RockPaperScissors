using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class GameSessionTests
{
    [Fact]
    public void IsFinished_AfterExactlyTotalRounds_BecomesTrue()
    {
        var session = new GameSession(3, new RecordingComputerPlayer());

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
        var session = new GameSession(1, new RecordingComputerPlayer());
        session.PlayRound(Move.Rock);

        Assert.Throws<InvalidOperationException>(() => session.PlayRound(Move.Rock));
        Assert.Single(session.History);
    }

    [Fact]
    public void PlayRound_AnyMove_ReturnsRoundWithBothMovesAndAddsItToHistory()
    {
        var session = new GameSession(1, new RecordingComputerPlayer(Move.Scissors));

        Round round = session.PlayRound(Move.Rock);

        Assert.Equal(new Round(Move.Rock, Move.Scissors), round);
        Assert.Equal(Outcome.Win, round.Outcome);
        Assert.Equal([round], session.History);
    }

    [Fact]
    public void Score_MixedOutcomes_MatchesHistory()
    {
        // Computer plays Scissors, Paper, Rock, Scissors against four Rocks: win, lose, draw, win.
        var session = new GameSession(4, new RecordingComputerPlayer(Move.Scissors, Move.Paper, Move.Rock, Move.Scissors));

        for (int round = 0; round < 4; round++)
        {
            session.PlayRound(Move.Rock);
        }

        Assert.Equal(2, session.Wins);
        Assert.Equal(1, session.Losses);
        Assert.Equal(1, session.Draws);
        Assert.Equal(session.History.Count, session.Wins + session.Losses + session.Draws);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(GameSession.MaxRounds + 1)]
    public void Constructor_InvalidTotalRounds_Throws(int totalRounds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSession(totalRounds, new RecordingComputerPlayer()));
    }

    [Fact]
    public void Constructor_MaxRounds_IsAllowed()
    {
        Assert.Equal(GameSession.MaxRounds, new GameSession(GameSession.MaxRounds, new RecordingComputerPlayer()).TotalRounds);
    }

    [Fact]
    public void PlayRound_EachRound_ComputerOnlyReceivesCompletedRounds()
    {
        var computer = new RecordingComputerPlayer();
        var session = new GameSession(3, computer);
        Move[] playerMoves = [Move.Paper, Move.Scissors, Move.Paper];

        foreach (Move move in playerMoves)
        {
            session.PlayRound(move);
        }

        // In round k the computer saw exactly the k-1 rounds before it, never the current player move.
        Assert.Equal([0, 1, 2], computer.ReceivedHistories.Select(history => history.Count));
        for (int k = 0; k < playerMoves.Length; k++)
        {
            Assert.Equal(session.History.Take(k), computer.ReceivedHistories[k]);
        }
    }

    [Fact]
    public void PlayRound_ComputerWithPredictors_RecordsPredictionsInRound()
    {
        var computer = new ComputerPlayer(
            [FakePredictor.Always(Move.Scissors), FakePredictor.Always(null)], new FakeRandom(), explorationRate: 0.0);
        var session = new GameSession(1, computer);

        Round round = session.PlayRound(Move.Rock);

        Assert.Equal([Move.Scissors, null], round.Predictions);
    }

    [Fact]
    public void PlayRound_EachRound_RecordedPredictionWasMadeWithoutCurrentPlayerMove()
    {
        // Predicts "the player's move in the latest round it can see", which exposes what it was shown.
        var predictor = new FakePredictor(history => history.Count == 0 ? null : history[^1].Player);
        var session = new GameSession(3, new ComputerPlayer([predictor], new FakeRandom(), explorationRate: 0.0));
        Move[] playerMoves = [Move.Paper, Move.Scissors, Move.Rock];

        foreach (Move move in playerMoves)
        {
            session.PlayRound(move);
        }

        // Round k recorded a prediction based on round k-1 only, never on its own move.
        Assert.Equal([null, Move.Paper, Move.Scissors], session.History.Select(round => round.Predictions[0]));
        Assert.Equal([0, 1, 2], predictor.ReceivedHistories.Select(history => history.Count));
    }

    [Fact]
    public void PlayRound_HistoryGivenToComputer_CannotBeModified()
    {
        var computer = new RecordingComputerPlayer();
        var session = new GameSession(1, computer);

        session.PlayRound(Move.Rock);

        Assert.IsNotType<List<Round>>(computer.ReceivedInstances[0]);
        Assert.True(((ICollection<Round>)computer.ReceivedInstances[0]).IsReadOnly);
    }
}
