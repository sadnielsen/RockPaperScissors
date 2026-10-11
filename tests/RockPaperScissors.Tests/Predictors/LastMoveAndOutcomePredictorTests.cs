using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Predictors;

public sealed class LastMoveAndOutcomePredictorTests
{
    private readonly LastMoveAndOutcomePredictor _predictor = new();

    [Fact]
    public void Predict_EmptyHistory_ReturnsNull()
    {
        Assert.Null(_predictor.Predict([]));
    }

    [Fact]
    public void Predict_SameMoveWithDifferentOutcome_DistinguishesByOutcome()
    {
        IReadOnlyList<Round> history =
        [
            new(Move.Rock, Move.Scissors), // Rock wins, so the player stays on Rock.
            new(Move.Rock, Move.Paper),    // Rock loses, so the player shifts to Paper.
            new(Move.Paper, Move.Rock),
            new(Move.Rock, Move.Scissors), // Rock wins again.
        ];

        // Only the earlier "Rock + Win" situation counts, not "Rock + Lose".
        Assert.Equal(Move.Rock, _predictor.Predict(history));
    }

    [Fact]
    public void Predict_SharedLead_ReturnsNull()
    {
        // After "Rock + Win" came Paper once and Scissors once.
        IReadOnlyList<Round> history =
        [
            new(Move.Rock, Move.Scissors),
            new(Move.Paper, Move.Paper),
            new(Move.Rock, Move.Scissors),
            new(Move.Scissors, Move.Paper),
            new(Move.Rock, Move.Scissors),
        ];

        Assert.Null(_predictor.Predict(history));
    }
}
