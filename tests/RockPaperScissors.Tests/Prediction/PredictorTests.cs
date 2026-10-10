using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Prediction;

public sealed class PredictorTests
{
    private const int Seed = 42;

    private readonly TransitionTable<LastMoveKey> _table = new(new Random(Seed));
    private readonly Predictor<LastMoveKey> _predictor;

    public PredictorTests()
    {
        _predictor = new Predictor<LastMoveKey>(new LastMoveContext(), _table);
    }

    [Fact]
    public void Predict_TooLittleHistory_ReturnsNull()
    {
        Assert.Null(_predictor.Predict(TestHistory.Empty));
    }

    [Fact]
    public void Learn_HistoryWithKey_RecordsActualMoveUnderThatKey()
    {
        _predictor.Learn(TestHistory.Of((Move.Rock, Move.Rock)), Move.Paper);

        Assert.Equal(2, _table.CountOf(new LastMoveKey(Move.Rock), Move.Paper));
    }

    [Fact]
    public void Learn_TooLittleHistory_RecordsNothing()
    {
        _predictor.Learn(TestHistory.Empty, Move.Paper);

        Assert.All(Enum.GetValues<Move>(), last =>
            Assert.Equal(TransitionTable<LastMoveKey>.InitialCount, _table.CountOf(new LastMoveKey(last), Move.Paper)));
    }

    [Fact]
    public void Predict_AfterLearning_ReturnsLearnedFollowUp()
    {
        var afterRock = TestHistory.Of((Move.Rock, Move.Rock));
        _predictor.Learn(afterRock, Move.Scissors);
        _predictor.Learn(afterRock, Move.Scissors);

        Assert.Equal(Move.Scissors, _predictor.Predict(afterRock));
    }
}
