namespace RockPaperScissors.Core;

/// <summary>The only place that knows which move beats which.</summary>
public static class Rules
{
    // A "beats" table instead of a full outcome matrix: every pair appears once,
    // so the table cannot claim both "Rock loses to Paper" and "Paper loses to Rock".
    private static readonly IReadOnlyDictionary<Move, Move> Beats = new Dictionary<Move, Move>
    {
        [Move.Rock] = Move.Scissors,
        [Move.Paper] = Move.Rock,
        [Move.Scissors] = Move.Paper,
    };

    public static Outcome Resolve(Move player, Move computer)
    {
        if (player == computer)
        {
            return Outcome.Draw;
        }

        return Beats[player] == computer ? Outcome.Win : Outcome.Lose;
    }
}
