using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Simulation;

/// <summary>
/// Plays full games against scripted opponents, with the same composition as the real program.
/// Seeded, so the results are identical on every run.
/// </summary>
/// <remarks>
/// A game of <see cref="GameSession.MaxRounds"/> takes seconds because hit scores are replayed every round
/// (the accepted price of deriving everything from the history). Each opponent therefore has its own
/// test class, so xUnit runs them in parallel; filter them out with <c>--filter Category!=Simulation</c>.
/// </remarks>
internal static class Simulator
{
    public const string Category = "Simulation";
    public const int Rounds = GameSession.MaxRounds;

    private const int ComputerSeed = 1;

    public static GameSession Play(ISimulatedPlayer player)
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

        return session;
    }

    // Session scores are from the player's side: a player loss is a computer win.
    public static double ComputerWinRate(GameSession session) => (double)session.Losses / Rounds;

    public static string Describe(GameSession session) =>
        $"Computer won {session.Losses}, lost {session.Wins}, drew {session.Draws} of {Rounds} rounds";
}
