using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Simulation;

internal sealed class RandomPlayer(Random random) : ISimulatedPlayer
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    public Move NextMove(IReadOnlyList<Round> history) => Moves[random.Next(Moves.Length)];
}
