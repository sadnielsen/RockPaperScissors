using RockPaperScissors.Core;

namespace RockPaperScissors.Web;

/// <summary>
/// Creates game sessions for one circuit. Registered as scoped, so every browser connection
/// gets its own Random and its own learning computer.
/// </summary>
public sealed class GameSessionFactory
{
    private readonly Random _random;
    private readonly Rules _rules;

    public GameSessionFactory(Random random, Rules rules)
    {
        _random = random;
        _rules = rules;
    }

    public GameSession Create(int totalRounds) =>
        new(totalRounds, StrategyFactory.CreateDefault(_random, _rules), _rules);
}
