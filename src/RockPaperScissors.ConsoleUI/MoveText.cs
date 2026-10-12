using RockPaperScissors.Core;

namespace RockPaperScissors.ConsoleUI;

/// <summary>
/// Translates moves to and from Dutch text. The one place in the console that names moves:
/// a new move only needs an entry here.
/// </summary>
public static class MoveText
{
    private sealed record Names(string Full, string Short);

    private static readonly IReadOnlyDictionary<Move, Names> NamesByMove = new Dictionary<Move, Names>
    {
        [Move.Rock] = new("rots", "r"),
        [Move.Paper] = new("papier", "p"),
        [Move.Scissors] = new("schaar", "s"),
    };

    public static string NameOf(Move move) => NamesByMove[move].Full;

    /// <summary>Accepts the full or short name, ignoring case and surrounding spaces.</summary>
    public static Move? Parse(string? input)
    {
        string text = input?.Trim() ?? string.Empty;

        foreach ((Move move, Names names) in NamesByMove)
        {
            if (text.Equals(names.Full, StringComparison.OrdinalIgnoreCase)
                || text.Equals(names.Short, StringComparison.OrdinalIgnoreCase))
            {
                return move;
            }
        }

        return null;
    }

    /// <summary>For prompts, e.g. "r = rots, p = papier, s = schaar".</summary>
    public static string Choices() =>
        string.Join(", ", Enum.GetValues<Move>().Select(move => $"{NamesByMove[move].Short} = {NamesByMove[move].Full}"));
}
