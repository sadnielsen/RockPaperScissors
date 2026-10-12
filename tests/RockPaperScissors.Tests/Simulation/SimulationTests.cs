using RockPaperScissors.Core;
using Xunit.Abstractions;

namespace RockPaperScissors.Tests.Simulation;

/// <summary>
/// Full games of the real opponents against scripted players.
/// Seeded, so the results are identical on every run.
/// </summary>
public sealed class SimulationTests(ITestOutputHelper output)
{
    private const int Rounds = GameSession.MaxRounds;
    private const int ComputerSeed = 1;
    private const int RandomPlayerSeed = 2;

    // The ceiling against a predictable player is about 87%: exploring rounds (20%) are won only a third
    // of the time. Measured: 86.8% for "Vorige zet" against the cycle, 86.6% for "Zet + uitkomst" against
    // win-stay, lose-shift. The margin keeps the tests meaningful without tying them to one exact run.
    private const double MinimumWinRateAgainstCycle = 0.75;
    private const double MinimumWinRateAgainstWinStayLoseShift = 0.70;

    // A random player cannot be predicted, so every outcome should be close to a third.
    // Over 1000 rounds one standard deviation is about 1.5%; these bounds are about 5 of them.
    // Measured: every share of every opponent between 30.5% and 36.3%.
    private const double MinimumShareAgainstRandom = 0.25;
    private const double MaximumShareAgainstRandom = 0.42;

    public static TheoryData<string> OpponentNames => new(Opponents.All.Select(option => option.Name));

    [Fact]
    public void Play_LastMoveAgainstFixedCycle_ComputerWinsComfortably()
    {
        GameSession session = Play(Opponent("Vorige zet"), new CyclePlayer());

        Assert.True(ComputerWinRate(session) > MinimumWinRateAgainstCycle, Describe(session));
    }

    [Fact]
    public void Play_DefaultAgainstWinStayLoseShift_ComputerWinsClearly()
    {
        GameSession session = Play(Opponents.Default, new WinStayLoseShiftPlayer());

        Assert.True(ComputerWinRate(session) > MinimumWinRateAgainstWinStayLoseShift, Describe(session));
        Assert.True(session.Losses > session.Wins, Describe(session));
    }

    [Theory]
    [MemberData(nameof(OpponentNames))]
    public void Play_AnyOpponentAgainstRandomPlayer_OutcomesStayAroundOneThird(string name)
    {
        GameSession session = Play(Opponent(name), new RandomPlayer(new Random(RandomPlayerSeed)));

        Assert.All(
            [session.Wins, session.Losses, session.Draws],
            count => Assert.InRange((double)count / Rounds, MinimumShareAgainstRandom, MaximumShareAgainstRandom));
    }

    private static OpponentOption Opponent(string name) => Opponents.All.Single(option => option.Name == name);

    private GameSession Play(OpponentOption opponent, ISimulatedPlayer player)
    {
        var session = new GameSession(Rounds, opponent.Create(new Random(ComputerSeed)));

        while (!session.IsFinished)
        {
            session.PlayRound(player.NextMove(session.History));
        }

        output.WriteLine($"{opponent.Name}: {Describe(session)}");
        return session;
    }

    // Session scores are from the player's side: a player loss is a computer win.
    private static double ComputerWinRate(GameSession session) => (double)session.Losses / Rounds;

    private static string Describe(GameSession session) =>
        $"Computer won {session.Losses}, lost {session.Wins}, drew {session.Draws} of {Rounds} rounds";
}
