using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Predictors;

public sealed class LastMovePredictorTests
{
    private readonly LastMovePredictor _predictor = new();

    [Fact]
    public void Predict_EmptyHistory_ReturnsNull()
    {
        Assert.Null(_predictor.Predict([]));
    }

    [Fact]
    public void Predict_SingleRound_ReturnsNull()
    {
        Assert.Null(_predictor.Predict(HistoryTestData.FromPlayerMoves(Move.Rock)));
    }

    [Fact]
    public void Predict_FixedCycle_ReturnsNextMoveInCycle()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Scissors, Move.Rock, Move.Paper);

        // After Paper the player has always played Scissors.
        Assert.Equal(Move.Scissors, _predictor.Predict(history));
    }

    [Fact]
    public void Predict_SharedLeadAfterLastMove_ReturnsNull()
    {
        // After Rock came Paper once and Scissors once.
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Rock, Move.Scissors, Move.Rock);

        Assert.Null(_predictor.Predict(history));
    }
}
