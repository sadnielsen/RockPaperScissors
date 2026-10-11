namespace RockPaperScissors.Core;

/// <summary>
/// One game of a fixed number of rounds. Owns the history, the only state in the game;
/// everything else is derived from it. Has no loop and does no I/O, so every interface can drive it.
/// </summary>
public sealed class GameSession
{
    /// <summary>Upper limit for a game; also keeps the replaying of hit scores cheap.</summary>
    public const int MaxRounds = 1000;

    private readonly IComputerPlayer _computer;
    private readonly List<Round> _history = [];

    public GameSession(int totalRounds, IComputerPlayer computer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(totalRounds, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalRounds, MaxRounds);
        ArgumentNullException.ThrowIfNull(computer);

        TotalRounds = totalRounds;
        _computer = computer;
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

        // The computer chooses first and only sees completed rounds: playerMove cannot reach it.
        Move computerMove = _computer.ChooseMove(History);

        var round = new Round(playerMove, computerMove);
        _history.Add(round);
        return round;
    }

    private int Count(Outcome outcome) => History.Count(round => round.Outcome == outcome);
}
