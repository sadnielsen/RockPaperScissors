using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Predictors;

public sealed class FrequencyPredictorTests
{
    private readonly FrequencyPredictor _predictor = new();

    [Fact]
    public void Predict_EmptyHistory_ReturnsNull()
    {
        Assert.Null(_predictor.Predict([]));
    }

    [Fact]
    public void Predict_OneMovePlayedMost_ReturnsThatMove()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Rock, Move.Scissors, Move.Rock);

        Assert.Equal(Move.Rock, _predictor.Predict(history));
    }

    [Fact]
    public void Predict_SharedLead_ReturnsNull()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Paper, Move.Rock);

        Assert.Null(_predictor.Predict(history));
    }
}
