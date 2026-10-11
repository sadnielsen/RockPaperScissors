using RockPaperScissors.Core;
using Xunit.Abstractions;

namespace RockPaperScissors.Tests.Simulation;

[Trait("Category", Simulator.Category)]
public sealed class CycleSimulationTests(ITestOutputHelper output)
{
    // The ceiling is about 87%: exploring rounds (20%) are won only a third of the time.
    // Measured: 86.7%. The margin keeps the test meaningful without tying it to one exact run.
    private const double MinimumComputerWinRate = 0.75;

    [Fact]
    public void Play_AgainstFixedCycle_ComputerWinsComfortably()
    {
        GameSession session = Simulator.Play(new CyclePlayer());
        output.WriteLine(Simulator.Describe(session));

        Assert.True(Simulator.ComputerWinRate(session) > MinimumComputerWinRate, Simulator.Describe(session));
    }
}
