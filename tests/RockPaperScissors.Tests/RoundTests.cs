using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class RoundTests
{
    [Fact]
    public void Outcome_AnyRound_MatchesRulesResolve()
    {
        foreach (Move player in Enum.GetValues<Move>())
        {
            foreach (Move computer in Enum.GetValues<Move>())
            {
                Assert.Equal(Rules.Resolve(player, computer), new Round(player, computer).Outcome);
            }
        }
    }
}
