namespace RockPaperScissors.Core.Predictors;

/// <summary>Catches longer sequences than <see cref="LastMovePredictor"/> can see.</summary>
public sealed class LastTwoMovesPredictor : IPredictor
{
    public Move? Predict(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count < 2)
        {
            return null;
        }

        Move secondLast = history[^2].Player;
        Move last = history[^1].Player;

        // Round 1 has only one round before it, so it can never match two moves.
        return PatternCounter.MostFrequentNext(
            history,
            i => i >= 2 && history[i - 2].Player == secondLast && history[i - 1].Player == last);
    }
}
