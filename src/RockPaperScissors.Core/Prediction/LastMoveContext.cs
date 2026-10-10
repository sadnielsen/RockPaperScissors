namespace RockPaperScissors.Core.Prediction;

/// <summary>Assumes the player's next move depends on their previous move ("after rock comes paper").</summary>
public sealed class LastMoveContext : IContext<LastMoveKey>
{
    public LastMoveKey? GetKey(IReadOnlyList<Round> history)
    {
        if (history.Count < 1)
        {
            return null;
        }

        return new LastMoveKey(history[^1].Player);
    }
}
