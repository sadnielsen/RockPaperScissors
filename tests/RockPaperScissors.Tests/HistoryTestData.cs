using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

/// <summary>Builds histories from player moves only, for tests where the computer's moves don't matter.</summary>
internal static class HistoryTestData
{
    public static IReadOnlyList<Round> FromPlayerMoves(params Move[] playerMoves) =>
        playerMoves.Select(move => new Round(move, Move.Rock)).ToList();
}
