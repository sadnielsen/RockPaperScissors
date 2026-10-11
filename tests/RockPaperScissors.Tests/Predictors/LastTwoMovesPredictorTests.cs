using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Predictors;

public sealed class LastTwoMovesPredictorTests
{
    private readonly LastTwoMovesPredictor _predictor = new();

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
    public void Predict_RepeatedSequence_ReturnsMoveThatFollowedLastTwo()
    {
        // The sequence Rock, Rock, Paper repeats. The last move alone (Rock) is ambiguous,
        // but the last two (Rock, Rock) were always followed by Paper.
        var history = HistoryTestData.FromPlayerMoves(
            Move.Rock, Move.Rock, Move.Paper,
            Move.Rock, Move.Rock, Move.Paper,
            Move.Rock, Move.Rock);

        Assert.Equal(Move.Paper, _predictor.Predict(history));
    }

    [Fact]
    public void Predict_SharedLead_ReturnsNull()
    {
        // After Rock, Paper came Scissors once and Rock once.
        var history = HistoryTestData.FromPlayerMoves(
            Move.Rock, Move.Paper, Move.Scissors,
            Move.Rock, Move.Paper, Move.Rock,
            Move.Paper);

        Assert.Null(_predictor.Predict(history));
    }
}
