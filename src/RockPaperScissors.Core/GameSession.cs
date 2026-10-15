namespace RockPaperScissors.Core;

/// <summary>
/// One game of a fixed number of rounds. Keeps the played rounds; the score is derived from them.
/// Has no loop and does no I/O, so the console (and tests) can drive it.
/// </summary>
public sealed class GameSession
{
    /// <summary>A game needs at least one round to have a winner.</summary>
    public const int MinRounds = 1;

    /// <summary>Upper limit for a game, so an absurd number from the player is rejected.</summary>
    public const int MaxRounds = 1000;

    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly Random _random;
    private readonly List<Round> _history = [];

    /// <param name="random">Injected, so tests can control the computer's moves.</param>
    public GameSession(int totalRounds, Random random)
    {
        if (!IsValidTotalRounds(totalRounds))
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalRounds), totalRounds, $"Must be between {MinRounds} and {MaxRounds}.");
        }

        ArgumentNullException.ThrowIfNull(random);

        TotalRounds = totalRounds;
        _random = random;
        History = _history.AsReadOnly();
    }

    /// <summary>
    /// Lets the console reject bad input by asking again, while the constructor still
    /// guards the same range against programming errors.
    /// </summary>
    public static bool IsValidTotalRounds(int totalRounds) =>
        totalRounds is >= MinRounds and <= MaxRounds;

    public int TotalRounds { get; }

    public IReadOnlyList<Round> History { get; }

    public bool IsFinished => History.Count >= TotalRounds;

    public int Wins => Count(Outcome.Win);

    public int Losses => Count(Outcome.Lose);

    public int Draws => Count(Outcome.Draw);

    /// <summary>The game's outcome for the player. Only defined once all rounds are played.</summary>
    /// <remarks>Throws while the game runs, so a half-played game is never reported as won or lost.</remarks>
    public Outcome Result
    {
        get
        {
            if (!IsFinished)
            {
                throw new InvalidOperationException(
                    $"The game has no result before all {TotalRounds} rounds are played.");
            }

            return Wins.CompareTo(Losses) switch
            {
                > 0 => Outcome.Win,
                < 0 => Outcome.Lose,
                _ => Outcome.Draw,
            };
        }
    }

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
