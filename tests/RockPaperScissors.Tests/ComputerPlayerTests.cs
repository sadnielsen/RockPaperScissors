using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class ComputerPlayerTests
{
    private const double NoExploration = 0.0;

    private static readonly IReadOnlyList<Round> RockOnlyHistory =
        HistoryTestData.FromPlayerMoves(Move.Rock, Move.Rock, Move.Rock);

    [Fact]
    public void ChooseMove_NoPredictors_PlaysRandomMoveFromAllMoves()
    {
        var random = new FakeRandom(ints: 2);
        var computer = new ComputerPlayer([], random, NoExploration);

        Move move = computer.ChooseMove(RockOnlyHistory);

        Assert.Equal(Enum.GetValues<Move>()[2], move);
        Assert.Equal([Enum.GetValues<Move>().Length], random.RequestedMaxValues);
    }

    [Fact]
    public void ChooseMove_PredictorPredictsRock_PlaysPaper()
    {
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], new FakeRandom(), NoExploration);

        Assert.Equal(Move.Paper, computer.ChooseMove(RockOnlyHistory));
    }

    [Fact]
    public void ChooseMove_PredictorReturnsNull_PlaysRandomMove()
    {
        var random = new FakeRandom(ints: 1);
        var computer = new ComputerPlayer([FakePredictor.Always(null)], random, NoExploration);

        Assert.Equal(Enum.GetValues<Move>()[1], computer.ChooseMove(RockOnlyHistory));
    }

    [Fact]
    public void ChooseMove_RollBelowExplorationRate_PlaysRandomMoveWithoutAskingPredictors()
    {
        var predictor = FakePredictor.Always(Move.Rock);
        var random = new FakeRandom(nextDouble: 0.1, ints: 0);
        var computer = new ComputerPlayer([predictor], random, ComputerPlayer.DefaultExplorationRate);

        Move move = computer.ChooseMove(RockOnlyHistory);

        Assert.Equal(Enum.GetValues<Move>()[0], move);
        Assert.Empty(predictor.ReceivedHistories);
    }

    [Fact]
    public void ChooseMove_RollAtExplorationRate_FollowsPredictor()
    {
        var random = new FakeRandom(nextDouble: ComputerPlayer.DefaultExplorationRate);
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], random, ComputerPlayer.DefaultExplorationRate);

        Assert.Equal(Move.Paper, computer.ChooseMove(RockOnlyHistory));
    }

    [Fact]
    public void ChooseMove_PredictorsWithDifferentHitScores_FollowsBestPredictor()
    {
        // The player only plays Rock: the second predictor is always right, the first never.
        var neverRight = FakePredictor.Always(Move.Scissors);
        var alwaysRight = FakePredictor.Always(Move.Rock);
        var computer = new ComputerPlayer([neverRight, alwaysRight], new FakeRandom(), NoExploration);

        Assert.Equal(Move.Paper, computer.ChooseMove(RockOnlyHistory));
    }

    [Fact]
    public void ChooseMove_EqualHitScores_FollowsFirstPredictor()
    {
        var first = FakePredictor.Always(Move.Paper);
        var second = FakePredictor.Always(Move.Scissors);
        var computer = new ComputerPlayer([first, second], new FakeRandom(), NoExploration);

        // Both scored 0; the counter of Paper is Scissors.
        Assert.Equal(Move.Scissors, computer.ChooseMove(RockOnlyHistory));
    }

    [Fact]
    public void ChooseMove_ReplayingHistory_PredictorOnlySeesRoundsBeforeEachRound()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Scissors, Move.Rock);
        var predictor = FakePredictor.Always(Move.Rock);
        var computer = new ComputerPlayer([predictor], new FakeRandom(), NoExploration);

        computer.ChooseMove(history);

        // Replay for rounds 1..3, then the real prediction on the whole history.
        Assert.Equal([1, 2, 3, 4], predictor.ReceivedHistories.Select(received => received.Count));
        Assert.All(predictor.ReceivedHistories, received => Assert.Equal(history.Take(received.Count), received));
    }

    [Fact]
    public void ChooseMove_SeededRandomWithFullExploration_PlaysEveryMove()
    {
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], new Random(42), explorationRate: 1.0);

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove(RockOnlyHistory)).ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), moves);
    }

    [Fact]
    public void ChooseMove_SeededRandomWithoutExploration_AlwaysPlaysCounter()
    {
        var computer = new ComputerPlayer([FakePredictor.Always(Move.Rock)], new Random(42), NoExploration);

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove(RockOnlyHistory)).ToHashSet();

        Assert.Equal([Move.Paper], moves);
    }

    [Fact]
    public void HitScore_PredictorRightTwice_ReturnsTwo()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Paper, Move.Rock, Move.Paper, Move.Rock);

        Assert.Equal(2, ComputerPlayer.HitScore(FakePredictor.Always(Move.Rock), history));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_ExplorationRateOutsideZeroToOne_Throws(double explorationRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ComputerPlayer([], new FakeRandom(), explorationRate));
    }
}
