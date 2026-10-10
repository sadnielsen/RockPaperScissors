namespace RockPaperScissors.Core;

/// <summary>
/// Plays uniformly at random: the game-theoretic equilibrium. It cannot be exploited,
/// but on average it does not win either.
/// </summary>
public sealed class RandomStrategy : IStrategy
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly Random _random;

    // Random is injected so tests can seed it and a single instance lives for the whole game.
    public RandomStrategy(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        _random = random;
    }

    public Move ChooseMove(IReadOnlyList<Round> history) => Moves[_random.Next(Moves.Length)];
}
