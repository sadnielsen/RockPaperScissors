namespace RockPaperScissors.Core.Prediction;

/// <summary>Assumes the player's next move depends on their last two moves, which captures longer sequences.</summary>
public sealed class Last2MovesContext : IContext<Last2MovesKey>
{
    public Last2MovesKey? GetKey(IReadOnlyList<Round> history)
    {
        if (history.Count < 2)
        {
            return null;
        }

        return new Last2MovesKey(history[^2].Player, history[^1].Player);
    }
}
