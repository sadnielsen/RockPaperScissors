using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Core;

/// <summary>
/// Follows whichever predictor would have been right most often so far, and plays a counter to its prediction.
/// Holds no game state: every choice is a function of the history and the random numbers.
/// </summary>
public sealed class ComputerPlayer : IComputerPlayer
{
    /// <summary>Chance of a random move, so the computer itself does not become predictable.</summary>
    public const double DefaultExplorationRate = 0.2;

    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly IReadOnlyList<IPredictor> _predictors;
    private readonly Random _random;
    private readonly double _explorationRate;

    public ComputerPlayer(IEnumerable<IPredictor> predictors, Random random, double explorationRate)
    {
        ArgumentNullException.ThrowIfNull(predictors);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(explorationRate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(explorationRate, 1.0);

        _predictors = predictors.ToList();
        _random = random;
        _explorationRate = explorationRate;
    }

    public Move ChooseMove(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        if (_random.NextDouble() < _explorationRate)
        {
            return RandomMove();
        }

        Move? prediction = BestPredictor(history)?.Predict(history);
        if (prediction is null)
        {
            return RandomMove();
        }

        IReadOnlyList<Move> counters = Rules.CountersOf(prediction.Value);
        return counters[_random.Next(counters.Count)];
    }

    /// <summary>
    /// How often the predictor would have been right had it been followed from the start.
    /// For round i it only sees the rounds before i, so replaying is as fair as playing.
    /// </summary>
    internal static int HitScore(IPredictor predictor, IReadOnlyList<Round> history) =>
        Enumerable.Range(1, Math.Max(0, history.Count - 1))
            .Count(i => predictor.Predict(new HistoryPrefix(history, i)) == history[i].Player);

    // On equal scores the earlier predictor wins, which makes the order of the list meaningful:
    // put simple predictors first, they need the least data.
    private IPredictor? BestPredictor(IReadOnlyList<Round> history)
    {
        IPredictor? best = null;
        int bestScore = -1;

        foreach (IPredictor predictor in _predictors)
        {
            int score = HitScore(predictor, history);
            if (score > bestScore)
            {
                best = predictor;
                bestScore = score;
            }
        }

        return best;
    }

    private Move RandomMove() => Moves[_random.Next(Moves.Length)];
}
