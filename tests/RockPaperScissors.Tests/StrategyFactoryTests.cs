using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class StrategyFactoryTests
{
    private const int Seed = 42;
    private const int TotalRounds = 100;
    private const int MeasuredRounds = 60;

    private readonly Rules _rules = new();

    public static TheoryData<Move[]> PlayerCycles => new()
    {
        new[] { Move.Rock, Move.Paper, Move.Scissors },
        new[] { Move.Rock, Move.Rock, Move.Paper, Move.Paper },
        new[] { Move.Scissors, Move.Paper, Move.Paper },
    };

    [Theory]
    [MemberData(nameof(PlayerCycles))]
    public void CreateDefault_AgainstFixedCycle_WinsMostRoundsDespiteExploration(Move[] playerCycle)
    {
        var session = new GameSession(TotalRounds, StrategyFactory.CreateDefault(new Random(Seed), _rules), _rules);

        for (var round = 0; round < TotalRounds; round++)
        {
            session.PlayRound(playerCycle[round % playerCycle.Length]);
        }

        // Exploration (20% random play) caps the win rate, so the bar is lower than for a pure pattern strategy.
        var computerWins = session.History.TakeLast(MeasuredRounds).Count(r => r.Outcome == Outcome.Lose);
        Assert.True(computerWins >= MeasuredRounds * 0.6, $"Computer won only {computerWins} of the last {MeasuredRounds} rounds.");
    }
}
