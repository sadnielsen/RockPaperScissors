namespace RockPaperScissors.Core;

/// <summary>
/// One game of a fixed number of rounds. Keeps the played rounds; the score is derived from them.
/// Has no loop and does no I/O, so the console (and tests) can drive it.
/// </summary>
public sealed class GameSession
{
    /// <summary>Upper limit for a game, so an absurd number from the player is rejected.</summary>
    public const int MaxRounds = 1000;

    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly Random _random;
    private readonly List<Round> _history = [];

    /// <param name="random">Injected, so tests can control the computer's moves.</param>
    public GameSession(int totalRounds, Random random)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(totalRounds, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalRounds, MaxRounds);
        ArgumentNullException.ThrowIfNull(random);

        TotalRounds = totalRounds;
        _random = random;
        History = _history.AsReadOnly();
    }

    public int TotalRounds { get; }

    public IReadOnlyList<Round> History { get; }

    public bool IsFinished => History.Count >= TotalRounds;

    public int Wins => Count(Outcome.Win);

    public int Losses => Count(Outcome.Lose);

    public int Draws => Count(Outcome.Draw);

    public Round PlayRound(Move playerMove)
    {
        if (IsFinished)
        {
            throw new InvalidOperationException($"The game is finished after {TotalRounds} rounds.");
        }

        // The computer's move depends only on the random numbers, never on playerMove.
        Move computerMove = Moves[_random.Next(Moves.Length)];

        var round = new Round(playerMove, computerMove);
        _history.Add(round);
        return round;
    }

    // Derived instead of kept in counters, so the score can never disagree with the rounds.
    private int Count(Outcome outcome) => History.Count(round => round.Outcome == outcome);
}
