namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// Predicts the player's next move. Hides the key type of the underlying context, so predictors
/// with different contexts can live in one list.
/// </summary>
public interface IPredictor
{
    /// <returns>The predicted player move, or <c>null</c> when there is no prediction.</returns>
    Move? Predict(IReadOnlyList<Round> history);

    /// <summary>Learns that the player played <paramref name="actual"/> after <paramref name="historyBefore"/>.</summary>
    void Learn(IReadOnlyList<Round> historyBefore, Move actual);
}
