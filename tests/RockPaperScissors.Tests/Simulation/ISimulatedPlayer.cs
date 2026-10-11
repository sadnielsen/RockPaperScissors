using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Simulation;

/// <summary>A scripted opponent for simulations. Unlike the computer, it may see the whole history.</summary>
internal interface ISimulatedPlayer
{
    Move NextMove(IReadOnlyList<Round> history);
}
