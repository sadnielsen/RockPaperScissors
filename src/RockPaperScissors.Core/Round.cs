namespace RockPaperScissors.Core;

/// <param name="Predictions">
/// What each predictor of the computer expected before this round, in the computer's predictor order.
/// Recorded as a fact of that moment, like the computer's move, so hit scores are a count instead of a replay.
/// </param>
public sealed record Round(Move Player, Move Computer, IReadOnlyList<Move?> Predictions)
{
    /// <summary>A round without recorded predictions, as played by a computer without predictors.</summary>
    public Round(Move player, Move computer)
        : this(player, computer, [])
    {
    }

    // Derived instead of stored, so it can never disagree with the moves.
    public Outcome Outcome => Rules.Resolve(Player, Computer);

    // A record compares a list member by reference; a value type should compare by content.
    public bool Equals(Round? other) =>
        other is not null
        && Player == other.Player
        && Computer == other.Computer
        && Predictions.SequenceEqual(other.Predictions);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Player);
        hash.Add(Computer);
        foreach (Move? prediction in Predictions)
        {
            hash.Add(prediction);
        }

        return hash.ToHashCode();
    }
}
