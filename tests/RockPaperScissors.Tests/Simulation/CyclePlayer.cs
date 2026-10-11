using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Simulation;

/// <summary>Plays all moves in a fixed order: Rock, Paper, Scissors, Rock, ...</summary>
internal sealed class CyclePlayer : ISimulatedPlayer
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    public Move NextMove(IReadOnlyList<Round> history) => Moves[history.Count % Moves.Length];
}
