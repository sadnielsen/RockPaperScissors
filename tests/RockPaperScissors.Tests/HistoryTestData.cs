using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

internal static class HistoryTestData
{
    /// <summary>A history from player moves only, for tests where the computer's side doesn't matter.</summary>
    public static IReadOnlyList<Round> FromPlayerMoves(params Move[] playerMoves) =>
        playerMoves.Select(move => new Round(move, Move.Rock)).ToList();
}
