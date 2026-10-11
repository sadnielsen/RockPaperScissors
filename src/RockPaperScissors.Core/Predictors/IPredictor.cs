namespace RockPaperScissors.Core.Predictors;

/// <summary>
/// Predicts the player's next move from the rounds played so far.
/// A predictor never chooses a move and remembers nothing between calls.
/// </summary>
public interface IPredictor
{
    /// <returns>The predicted move, or null when the history is too short or shows no clear leader.</returns>
    Move? Predict(IReadOnlyList<Round> history);
}
