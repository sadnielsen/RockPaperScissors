using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class PredictingComputerPlayerTests
{
    private const double NoExploration = 0.0;

    private static readonly IReadOnlyList<Round> History = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper);

    [Fact]
    public void ChooseMove_PredictorPredictsRock_PlaysPaper()
    {
        var computer = new PredictingComputerPlayer(FakePredictor.Always(Move.Rock), new FakeRandom(), NoExploration);

        Assert.Equal(Move.Paper, computer.ChooseMove(History));
    }

    [Fact]
    public void ChooseMove_PredictorReturnsNull_PlaysRandomMove()
    {
        var random = new FakeRandom(ints: 1);
        var computer = new PredictingComputerPlayer(FakePredictor.Always(null), random, NoExploration);

        Assert.Equal(Enum.GetValues<Move>()[1], computer.ChooseMove(History));
        Assert.Equal([Enum.GetValues<Move>().Length], random.RequestedMaxValues);
    }

    [Fact]
    public void ChooseMove_AnyHistory_AsksPredictorOnceWithExactlyThatHistory()
    {
        var predictor = FakePredictor.Always(Move.Rock);
        var computer = new PredictingComputerPlayer(predictor, new FakeRandom(), NoExploration);

        computer.ChooseMove(History);

        Assert.Equal([History], predictor.ReceivedHistories);
    }

    [Fact]
    public void ChooseMove_RollBelowExplorationRate_PlaysRandomMove()
    {
        var random = new FakeRandom(nextDouble: 0.1, ints: 0);
        var computer = new PredictingComputerPlayer(
            FakePredictor.Always(Move.Rock), random, PredictingComputerPlayer.DefaultExplorationRate);

        Assert.Equal(Enum.GetValues<Move>()[0], computer.ChooseMove(History));
    }

    [Fact]
    public void ChooseMove_RollBelowExplorationRate_DoesNotAskPredictor()
    {
        var predictor = FakePredictor.Always(Move.Rock);
        var random = new FakeRandom(nextDouble: 0.1);
        var computer = new PredictingComputerPlayer(predictor, random, PredictingComputerPlayer.DefaultExplorationRate);

        computer.ChooseMove(History);

        Assert.Empty(predictor.ReceivedHistories);
    }

    [Fact]
    public void ChooseMove_RollAtExplorationRate_FollowsPredictor()
    {
        var random = new FakeRandom(nextDouble: PredictingComputerPlayer.DefaultExplorationRate);
        var computer = new PredictingComputerPlayer(
            FakePredictor.Always(Move.Rock), random, PredictingComputerPlayer.DefaultExplorationRate);

        Assert.Equal(Move.Paper, computer.ChooseMove(History));
    }

    [Fact]
    public void ChooseMove_SeededRandomWithFullExploration_PlaysEveryMove()
    {
        var computer = new PredictingComputerPlayer(FakePredictor.Always(Move.Rock), new Random(42), explorationRate: 1.0);

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove(History)).ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), moves);
    }

    [Fact]
    public void ChooseMove_SeededRandomWithoutExploration_AlwaysPlaysCounter()
    {
        var computer = new PredictingComputerPlayer(FakePredictor.Always(Move.Rock), new Random(42), NoExploration);

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove(History)).ToHashSet();

        Assert.Equal([Move.Paper], moves);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_ExplorationRateOutsideZeroToOne_Throws(double explorationRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PredictingComputerPlayer(FakePredictor.Always(null), new FakeRandom(), explorationRate));
    }
}
