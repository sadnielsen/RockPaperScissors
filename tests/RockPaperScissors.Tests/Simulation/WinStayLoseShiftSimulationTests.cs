using RockPaperScissors.Core;
using Xunit.Abstractions;

namespace RockPaperScissors.Tests.Simulation;

[Trait("Category", Simulator.Category)]
public sealed class WinStayLoseShiftSimulationTests(ITestOutputHelper output)
{
    // Measured: 86.0%, close to the ceiling of about 87% that exploration allows.
    private const double MinimumComputerWinRate = 0.70;

    [Fact]
    public void Play_AgainstWinStayLoseShift_ComputerWinsClearly()
    {
        GameSession session = Simulator.Play(new WinStayLoseShiftPlayer());
        output.WriteLine(Simulator.Describe(session));

        Assert.True(Simulator.ComputerWinRate(session) > MinimumComputerWinRate, Simulator.Describe(session));
        Assert.True(session.Losses > session.Wins, Simulator.Describe(session));
    }
}
