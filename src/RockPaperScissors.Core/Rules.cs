namespace RockPaperScissors.Core;

/// <summary>The only place that knows which move beats which.</summary>
public static class Rules
{
    // A "beats" table instead of a full outcome matrix: every pair appears once,
    // so the table cannot claim both "Rock loses to Paper" and "Paper loses to Rock".
    // Adding moves (e.g. Fire, Water) only means extending this table.
    private static readonly IReadOnlyDictionary<Move, IReadOnlySet<Move>> Beats =
        new Dictionary<Move, IReadOnlySet<Move>>
        {
            [Move.Rock] = new HashSet<Move> { Move.Scissors },
            [Move.Paper] = new HashSet<Move> { Move.Rock },
            [Move.Scissors] = new HashSet<Move> { Move.Paper },
        };

    public static Outcome Resolve(Move player, Move computer)
    {
        if (player == computer)
        {
            return Outcome.Draw;
        }

        return Beats[player].Contains(computer) ? Outcome.Win : Outcome.Lose;
    }

    /// <summary>The moves that beat <paramref name="move"/>.</summary>
    public static IReadOnlyList<Move> CountersOf(Move move) =>
        Enum.GetValues<Move>().Where(candidate => Beats[candidate].Contains(move)).ToList();
}
