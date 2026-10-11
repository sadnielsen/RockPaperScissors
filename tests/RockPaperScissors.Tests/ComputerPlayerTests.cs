using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class ComputerPlayerTests
{
    private const double NoExploration = 0.0;

    // Three Rock rounds, recorded by a computer with one predictor that had no prediction.
    private static readonly IReadOnlyList<Round> OnePredictorHistory =
        HistoryTestData.WithPredictions((Move.Rock, [null]), (Move.Rock, [null]), (Move.Rock, [null]));

    [Fact]
    public void ChooseMove_NoPredictors_PlaysRandomMoveFromAllMoves()
    {
        var random = new FakeRandom(ints: 2);
        var computer = new ComputerPlayer([], random, NoExploration);

        ComputerDecision decision = computer.ChooseMove(HistoryTestData.FromPlayerMoves(Move.Rock));

        Assert.Equal(Enum.GetValues<Move>()[2], decision.Move);
        Assert.Equal([Enum.GetValues<Move>().Length], random.RequestedMaxValues);
        Assert.Empty(decision.Predictions);
    }

    [Fact]
    public void ChooseMove_PredictorPredictsRock_PlaysPaper()
    {
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], new FakeRandom(), NoExploration);

        Assert.Equal(Move.Paper, computer.ChooseMove(OnePredictorHistory).Move);
    }

    [Fact]
    public void ChooseMove_PredictorReturnsNull_PlaysRandomMove()
    {
        var random = new FakeRandom(ints: 1);
        var computer = new ComputerPlayer([FakePredictor.Always(null)], random, NoExploration);

        Assert.Equal(Enum.GetValues<Move>()[1], computer.ChooseMove(OnePredictorHistory).Move);
    }

    [Fact]
    public void ChooseMove_RollBelowExplorationRate_PlaysRandomMove()
    {
        var random = new FakeRandom(nextDouble: 0.1, ints: 0);
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], random, ComputerPlayer.DefaultExplorationRate);

        Assert.Equal(Enum.GetValues<Move>()[0], computer.ChooseMove(OnePredictorHistory).Move);
    }

    [Fact]
    public void ChooseMove_RollBelowExplorationRate_StillRecordsPredictions()
    {
        var random = new FakeRandom(nextDouble: 0.1);
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Scissors)], random, ComputerPlayer.DefaultExplorationRate);

        Assert.Equal([Move.Scissors], computer.ChooseMove(OnePredictorHistory).Predictions);
    }

    [Fact]
    public void ChooseMove_RollAtExplorationRate_FollowsPredictor()
    {
        var random = new FakeRandom(nextDouble: ComputerPlayer.DefaultExplorationRate);
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], random, ComputerPlayer.DefaultExplorationRate);

        Assert.Equal(Move.Paper, computer.ChooseMove(OnePredictorHistory).Move);
    }

    [Fact]
    public void ChooseMove_PredictorsWithDifferentRecordedHits_FollowsBestPredictor()
    {
        // The player only played Rock: the second predictor was right twice, the first never.
        var history = HistoryTestData.WithPredictions(
            (Move.Rock, [null, null]),
            (Move.Rock, [Move.Paper, Move.Rock]),
            (Move.Rock, [Move.Scissors, Move.Rock]));
        IPredictor[] predictors = [FakePredictor.Always(Move.Paper), FakePredictor.Always(Move.Scissors)];
        var computer = new ComputerPlayer(predictors, new FakeRandom(), NoExploration);

        // Follows the second predictor's current prediction (Scissors), so plays Rock.
        Assert.Equal(Move.Rock, computer.ChooseMove(history).Move);
    }

    [Fact]
    public void ChooseMove_EqualRecordedHits_FollowsFirstPredictor()
    {
        var history = HistoryTestData.WithPredictions(
            (Move.Rock, [Move.Rock, null]),
            (Move.Paper, [null, Move.Paper]));
        IPredictor[] predictors = [FakePredictor.Always(Move.Paper), FakePredictor.Always(Move.Scissors)];
        var computer = new ComputerPlayer(predictors, new FakeRandom(), NoExploration);

        // Both were right once; the counter of the first predictor's Paper is Scissors.
        Assert.Equal(Move.Scissors, computer.ChooseMove(history).Move);
    }

    [Fact]
    public void ChooseMove_AnyHistory_ReturnsEachPredictorsPredictionInOrder()
    {
        IPredictor[] predictors = [FakePredictor.Always(Move.Paper), FakePredictor.Always(null), FakePredictor.Always(Move.Rock)];
        var computer = new ComputerPlayer(predictors, new FakeRandom(), NoExploration);

        ComputerDecision decision = computer.ChooseMove([]);

        Assert.Equal([Move.Paper, null, Move.Rock], decision.Predictions);
    }

    [Fact]
    public void ChooseMove_AnyHistory_AsksEachPredictorOnceWithExactlyThatHistory()
    {
        var first = FakePredictor.Always(Move.Rock);
        var second = FakePredictor.Always(null);
        var history = HistoryTestData.WithPredictions((Move.Rock, [null, null]), (Move.Paper, [Move.Rock, null]));
        var computer = new ComputerPlayer([first, second], new FakeRandom(), NoExploration);

        computer.ChooseMove(history);

        // No replaying: each predictor sees the whole history once, and nothing more.
        Assert.All([first, second], predictor => Assert.Equal([history], predictor.ReceivedHistories));
    }

    [Fact]
    public void ChooseMove_HistoryWithOtherNumberOfPredictions_Throws()
    {
        var computer = new ComputerPlayer(
            [FakePredictor.Always(Move.Rock), FakePredictor.Always(Move.Rock)], new FakeRandom(), NoExploration);

        Assert.Throws<ArgumentException>(() => computer.ChooseMove(OnePredictorHistory));
    }

    [Fact]
    public void ChooseMove_SeededRandomWithFullExploration_PlaysEveryMove()
    {
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], new Random(42), explorationRate: 1.0);

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove(OnePredictorHistory).Move).ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), moves);
    }

    [Fact]
    public void ChooseMove_SeededRandomWithoutExploration_AlwaysPlaysCounter()
    {
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], new Random(42), NoExploration);

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove(OnePredictorHistory).Move).ToHashSet();

        Assert.Equal([Move.Paper], moves);
    }

    [Fact]
    public void HitScore_PredictionsRightTwice_ReturnsTwo()
    {
        var history = HistoryTestData.WithPredictions(
            (Move.Paper, [null]),
            (Move.Rock, [Move.Rock]),
            (Move.Paper, [Move.Rock]),
            (Move.Rock, [Move.Rock]));

        Assert.Equal(2, ComputerPlayer.HitScore(history, predictorIndex: 0));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_ExplorationRateOutsideZeroToOne_Throws(double explorationRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ComputerPlayer([], new FakeRandom(), explorationRate));
    }
}
