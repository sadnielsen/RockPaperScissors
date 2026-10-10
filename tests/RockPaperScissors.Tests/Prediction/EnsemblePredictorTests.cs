using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Prediction;

public sealed class EnsemblePredictorTests
{
    private const int FirstTacticRounds = 20;
    private const int SecondTacticRounds = 10;

    private static readonly EnsembleOptions NoDecay = new() { Decay = 1.0 };

    [Fact]
    public void Predict_NoLearningYet_FollowsFirstPredictor()
    {
        var ensemble = new EnsemblePredictor(new[] { new FakePredictor(Move.Rock), new FakePredictor(Move.Paper) });

        Assert.Equal(Move.Rock, ensemble.Predict(TestHistory.Empty));
    }

    [Fact]
    public void Predict_AfterLearning_FollowsPredictorThatWasRightMostOften()
    {
        var ensemble = new EnsemblePredictor(new[] { new FakePredictor(Move.Rock), new FakePredictor(Move.Paper) });

        LearnRepeatedly(ensemble, Move.Paper, times: 3);

        Assert.Equal(Move.Paper, ensemble.Predict(TestHistory.Empty));
        Assert.True(ensemble.Scores[1] > ensemble.Scores[0]);
    }

    [Fact]
    public void Predict_PlayerSwitchesTactic_FollowsNewBestPredictorThanksToDecay()
    {
        var ensemble = new EnsemblePredictor(new[] { new FakePredictor(Move.Rock), new FakePredictor(Move.Paper) });

        LearnRepeatedly(ensemble, Move.Paper, FirstTacticRounds);
        LearnRepeatedly(ensemble, Move.Rock, SecondTacticRounds);

        Assert.Equal(Move.Rock, ensemble.Predict(TestHistory.Empty));
    }

    [Fact]
    public void Predict_PlayerSwitchesTacticWithoutDecay_KeepsFollowingOldPredictor()
    {
        var ensemble = new EnsemblePredictor(
            new[] { new FakePredictor(Move.Rock), new FakePredictor(Move.Paper) }, NoDecay);

        LearnRepeatedly(ensemble, Move.Paper, FirstTacticRounds);
        LearnRepeatedly(ensemble, Move.Rock, SecondTacticRounds);

        Assert.Equal(Move.Paper, ensemble.Predict(TestHistory.Empty));
    }

    [Fact]
    public void Predict_BestPredictorHasNoPrediction_FollowsBestPredictorThatHasOne()
    {
        var best = new FakePredictor(Move.Paper);
        var ensemble = new EnsemblePredictor(new[] { new FakePredictor(Move.Rock), best });
        LearnRepeatedly(ensemble, Move.Paper, times: 3);

        best.Prediction = null;

        Assert.Equal(Move.Rock, ensemble.Predict(TestHistory.Empty));
    }

    [Fact]
    public void Predict_NoPredictorHasPrediction_ReturnsNull()
    {
        var ensemble = new EnsemblePredictor(new[] { new FakePredictor(null), new FakePredictor(null) });

        Assert.Null(ensemble.Predict(TestHistory.Empty));
    }

    [Fact]
    public void Learn_AnyRound_ForwardsToEveryPredictor()
    {
        var first = new FakePredictor(Move.Rock);
        var second = new FakePredictor(null);
        var ensemble = new EnsemblePredictor(new[] { first, second });
        var history = TestHistory.Of((Move.Rock, Move.Paper));

        ensemble.Learn(history, Move.Scissors);

        Assert.All(new[] { first, second }, predictor =>
        {
            var learned = Assert.Single(predictor.Learned);
            Assert.Equal(history, learned.HistoryBefore);
            Assert.Equal(Move.Scissors, learned.Actual);
        });
    }

    [Fact]
    public void Constructor_NoPredictors_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new EnsemblePredictor(Array.Empty<IPredictor>()));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_DecayOutsideZeroToOne_ThrowsArgumentOutOfRangeException(double decay)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnsemblePredictor(
            new[] { new FakePredictor(null) }, new EnsembleOptions { Decay = decay }));
    }

    private static void LearnRepeatedly(EnsemblePredictor ensemble, Move actual, int times)
    {
        for (var i = 0; i < times; i++)
        {
            ensemble.Learn(TestHistory.Empty, actual);
        }
    }
}
