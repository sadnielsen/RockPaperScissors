using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Simulation;

/// <summary>Repeats a winning move; after a loss or draw shifts to the next move in order.</summary>
internal sealed class WinStayLoseShiftPlayer : ISimulatedPlayer
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    public Move NextMove(IReadOnlyList<Round> history)
    {
        if (history.Count == 0)
        {
            return Moves[0];
        }

        Round last = history[^1];
        if (last.Outcome == Outcome.Win)
        {
            return last.Player;
        }

        return Moves[(Array.IndexOf(Moves, last.Player) + 1) % Moves.Length];
    }
}
