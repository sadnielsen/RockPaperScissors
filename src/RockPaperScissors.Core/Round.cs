namespace RockPaperScissors.Core;

public sealed record Round(Move Player, Move Computer)
{
    // Derived instead of stored, so it can never disagree with the moves.
    public Outcome Outcome => Rules.Resolve(Player, Computer);
}
