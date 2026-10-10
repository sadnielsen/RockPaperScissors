using RockPaperScissors.Core;

namespace RockPaperScissors.Console;

/// <summary>Translates between the player's Dutch text and moves. Presentation only, no game rules.</summary>
public static class MoveText
{
    private static readonly Dictionary<string, Move> Inputs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["r"] = Move.Rock,
        ["rots"] = Move.Rock,
        ["p"] = Move.Paper,
        ["papier"] = Move.Paper,
        ["s"] = Move.Scissors,
        ["schaar"] = Move.Scissors,
    };

    public static bool TryParse(string? input, out Move move)
    {
        move = default;
        return input is not null && Inputs.TryGetValue(input.Trim(), out move);
    }

    public static string NameOf(Move move) => move switch
    {
        Move.Rock => "rots",
        Move.Paper => "papier",
        Move.Scissors => "schaar",
        _ => throw new ArgumentOutOfRangeException(nameof(move), move, "Unknown move."),
    };
}
