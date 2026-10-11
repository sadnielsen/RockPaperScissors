using RockPaperScissors.Core;
using Xunit.Abstractions;

namespace RockPaperScissors.Tests.Simulation;

[Trait("Category", Simulator.Category)]
public sealed class RandomPlayerSimulationTests(ITestOutputHelper output)
{
    private const int PlayerSeed = 2;

    // A random player cannot be predicted, so every outcome should be close to a third.
    // Over 1000 rounds one standard deviation is about 1.5%; these bounds are about 5 of them.
    // Measured: 35.3% / 31.8% / 32.9%.
    private const double MinimumShare = 0.25;
    private const double MaximumShare = 0.42;

    [Fact]
    public void Play_AgainstRandomPlayer_OutcomesStayAroundOneThird()
    {
        GameSession session = Simulator.Play(new RandomPlayer(new Random(PlayerSeed)));
        output.WriteLine(Simulator.Describe(session));

        Assert.All(
            [session.Wins, session.Losses, session.Draws],
            count => Assert.InRange((double)count / Simulator.Rounds, MinimumShare, MaximumShare));
    }
}
