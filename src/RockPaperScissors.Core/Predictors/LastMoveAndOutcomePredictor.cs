namespace RockPaperScissors.Core.Predictors;

/// <summary>
/// Catches reactions to the previous result, such as win-stay, lose-shift:
/// on average the most predictive pattern in human players.
/// </summary>
public sealed class LastMoveAndOutcomePredictor : IPredictor
{
    public Move? Predict(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count == 0)
        {
            return null;
        }

        Move lastMove = history[^1].Player;
        Outcome lastOutcome = history[^1].Outcome;
        return PatternCounter.MostFrequentNext(
            history,
            i => history[i - 1].Player == lastMove && history[i - 1].Outcome == lastOutcome);
    }
}
