namespace RockPaperScissors.Core;

/// <summary>
/// Plays a uniformly random move every round: the game-theoretic equilibrium.
/// It cannot be beaten in the long run, but it does not win either.
/// </summary>
public sealed class RandomComputerPlayer : IComputerPlayer
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly Random _random;

    public RandomComputerPlayer(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        _random = random;
    }

    public Move ChooseMove(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        return Moves[_random.Next(Moves.Length)];
    }
}
