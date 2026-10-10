namespace RockPaperScissors.Core;

/// <summary>
/// All game logic around rounds, without a loop or I/O. Interfaces only decide who drives
/// the rounds: the console loops over input, the web calls <see cref="PlayRound"/> per click.
/// </summary>
public sealed class GameSession
{
    public const int MinRounds = 1;

    // Guards against absurd input; no human plays more rounds than this in one sitting.
    public const int MaxRounds = 1000;

    private readonly IStrategy _strategy;
    private readonly Rules _rules;
    private readonly List<Round> _history = new();
    private Move? _committedComputerMove;

    public GameSession(int totalRounds, IStrategy strategy, Rules rules)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(totalRounds, MinRounds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalRounds, MaxRounds);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(rules);

        TotalRounds = totalRounds;
        _strategy = strategy;
        _rules = rules;
        // A read-only view, so neither callers nor the strategy can rewrite past rounds.
        History = _history.AsReadOnly();

        CommitComputerMove();
    }

    public int TotalRounds { get; }

    public IReadOnlyList<Round> History { get; }

    public Score Score { get; private set; }

    public bool IsFinished => _history.Count >= TotalRounds;

    public Round PlayRound(Move playerMove)
    {
        if (!Enum.IsDefined(playerMove))
        {
            throw new ArgumentOutOfRangeException(nameof(playerMove), playerMove, "Unknown move.");
        }

        if (IsFinished || _committedComputerMove is not { } computerMove)
        {
            throw new InvalidOperationException($"The game is finished after {TotalRounds} rounds.");
        }

        var round = new Round(playerMove, computerMove, _rules.Resolve(playerMove, computerMove));

        // No separate learn call: strategies derive what they learn from History on their next turn.
        _history.Add(round);
        Score = Score.Add(round.Outcome);
        CommitComputerMove();

        return round;
    }

    // The computer commits to its next move as soon as the previous round is done, so it has
    // chosen before any interface even asks the player. The move stays private: revealing it
    // would let the player cheat instead.
    private void CommitComputerMove()
    {
        _committedComputerMove = IsFinished ? null : _strategy.ChooseMove(History);
    }
}
