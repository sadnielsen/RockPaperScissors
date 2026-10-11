namespace RockPaperScissors.Core.Predictors;

/// <summary>Catches a preference for one move, e.g. a player who plays rock most of the time.</summary>
public sealed class FrequencyPredictor : IPredictor
{
    public Move? Predict(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        return PatternCounter.MostFrequent(history.Select(round => round.Player));
    }
}
