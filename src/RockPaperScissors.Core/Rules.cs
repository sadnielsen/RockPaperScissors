namespace RockPaperScissors.Core;

/// <summary>
/// The single source of truth for who beats whom. The rules are data (an outcome matrix),
/// so extending the game means editing the matrix, not adding branches.
/// </summary>
public sealed class Rules
{
    // Row = player's move, column = computer's move, indexed by the Move enum value.
    private static readonly Outcome[,] OutcomeMatrix =
    {
        //                 Rock          Paper         Scissors
        /* Rock     */ { Outcome.Draw, Outcome.Lose, Outcome.Win  },
        /* Paper    */ { Outcome.Win,  Outcome.Draw, Outcome.Lose },
        /* Scissors */ { Outcome.Lose, Outcome.Win,  Outcome.Draw },
    };

    public Outcome Resolve(Move player, Move computer) =>
        OutcomeMatrix[(int)player, (int)computer];

    /// <summary>Returns the move that beats <paramref name="move"/>.</summary>
    public Move CounterOf(Move move)
    {
        // Derived from the matrix instead of a separate lookup, so the two can never disagree.
        foreach (var candidate in Enum.GetValues<Move>())
        {
            if (Resolve(candidate, move) == Outcome.Win)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"The outcome matrix has no move that beats {move}.");
    }
}
