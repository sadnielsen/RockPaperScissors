namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// Assumes the player reacts to how the last round went, which captures win-stay, lose-shift:
/// on average the most predictive pattern in human players.
/// </summary>
public sealed class LastMoveWithOutcomeContext : IContext<LastMoveWithOutcomeKey>
{
    public LastMoveWithOutcomeKey? GetKey(IReadOnlyList<Round> history)
    {
        if (history.Count < 1)
        {
            return null;
        }

        var last = history[^1];
        return new LastMoveWithOutcomeKey(last.Player, last.Outcome);
    }
}
