using RockPaperScissors.Core;

namespace RockPaperScissors.Web;

/// <summary>Dutch display names. The one place in the web interface that names moves.</summary>
public static class MoveNames
{
    private static readonly IReadOnlyDictionary<Move, string> Names = new Dictionary<Move, string>
    {
        [Move.Rock] = "Rots",
        [Move.Paper] = "Papier",
        [Move.Scissors] = "Schaar",
    };

    public static string NameOf(Move move) => Names[move];
}
