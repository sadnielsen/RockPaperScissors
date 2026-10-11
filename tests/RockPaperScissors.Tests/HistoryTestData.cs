using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

internal static class HistoryTestData
{
    /// <summary>A history from player moves only, for tests where the computer's side doesn't matter.</summary>
    public static IReadOnlyList<Round> FromPlayerMoves(params Move[] playerMoves) =>
        playerMoves.Select(move => new Round(move, Move.Rock)).ToList();

    /// <summary>A history with explicit recorded predictions, one array per round in predictor order.</summary>
    public static IReadOnlyList<Round> WithPredictions(params (Move Player, Move?[] Predictions)[] rounds) =>
        rounds.Select(round => new Round(round.Player, Move.Rock, round.Predictions)).ToList();
}
