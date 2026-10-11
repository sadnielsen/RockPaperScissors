namespace RockPaperScissors.Core.Predictors;

/// <summary>Catches "after rock comes paper" habits and fixed cycles.</summary>
public sealed class LastMovePredictor : IPredictor
{
    public Move? Predict(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count == 0)
        {
            return null;
        }

        Move last = history[^1].Player;
        return PatternCounter.MostFrequentNext(history, i => history[i - 1].Player == last);
    }
}
