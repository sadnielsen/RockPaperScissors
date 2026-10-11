using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;
using Xunit.Abstractions;

namespace RockPaperScissors.Tests.Simulation;

/// <summary>
/// Full games against scripted opponents, with the same composition as the real program.
/// Seeded, so the results are identical on every run.
/// </summary>
public sealed class SimulationTests(ITestOutputHelper output)
{
    private const int Rounds = GameSession.MaxRounds;
    private const int ComputerSeed = 1;
    private const int RandomPlayerSeed = 2;

    // The ceiling against a predictable player is about 87%: exploring rounds (20%) are won only a third
    // of the time. Measured: 86.7% against the cycle, 86.0% against win-stay, lose-shift.
    // The margin keeps the tests meaningful without tying them to one exact run.
    private const double MinimumWinRateAgainstCycle = 0.75;
    private const double MinimumWinRateAgainstWinStayLoseShift = 0.70;

    // A random player cannot be predicted, so every outcome should be close to a third.
    // Over 1000 rounds one standard deviation is about 1.5%; these bounds are about 5 of them.
    // Measured: 35.3% / 31.8% / 32.9%.
    private const double MinimumShareAgainstRandom = 0.25;
    private const double MaximumShareAgainstRandom = 0.42;

    [Fact]
    public void Play_AgainstFixedCycle_ComputerWinsComfortably()
    {
        GameSession session = Play(new CyclePlayer());

        Assert.True(ComputerWinRate(session) > MinimumWinRateAgainstCycle, Describe(session));
    }

    [Fact]
    public void Play_AgainstWinStayLoseShift_ComputerWinsClearly()
    {
        GameSession session = Play(new WinStayLoseShiftPlayer());

        Assert.True(ComputerWinRate(session) > MinimumWinRateAgainstWinStayLoseShift, Describe(session));
        Assert.True(session.Losses > session.Wins, Describe(session));
    }

    [Fact]
    public void Play_AgainstRandomPlayer_OutcomesStayAroundOneThird()
    {
        GameSession session = Play(new RandomPlayer(new Random(RandomPlayerSeed)));

        Assert.All(
            [session.Wins, session.Losses, session.Draws],
            count => Assert.InRange((double)count / Rounds, MinimumShareAgainstRandom, MaximumShareAgainstRandom));
    }

    private GameSession Play(ISimulatedPlayer player)
    {
        var computer = new ComputerPlayer(
            [new FrequencyPredictor(), new LastMovePredictor(), new LastMoveAndOutcomePredictor(), new LastTwoMovesPredictor()],
            new Random(ComputerSeed),
            ComputerPlayer.DefaultExplorationRate);
        var session = new GameSession(Rounds, computer);

        while (!session.IsFinished)
        {
            session.PlayRound(player.NextMove(session.History));
        }

        output.WriteLine(Describe(session));
        return session;
    }

    // Session scores are from the player's side: a player loss is a computer win.
    private static double ComputerWinRate(GameSession session) => (double)session.Losses / Rounds;

    private static string Describe(GameSession session) =>
        $"Computer won {session.Losses}, lost {session.Wins}, drew {session.Draws} of {Rounds} rounds";
}
