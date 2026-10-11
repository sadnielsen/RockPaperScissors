using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Core;

/// <summary>
/// Follows whichever predictor has been right most often so far, and plays a counter to its prediction.
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

    public ComputerDecision ChooseMove(IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        EnsureRecordedByThisConfiguration(history);

        // Also predicted in exploring rounds: every round must record every prediction,
        // or later hit scores would miss rounds.
        IReadOnlyList<Move?> predictions = _predictors.Select(predictor => predictor.Predict(history)).ToList();

        return new ComputerDecision(ChooseMove(history, predictions), predictions);
    }

    /// <summary>
    /// How often the predictor at <paramref name="predictorIndex"/> was right so far:
    /// a count of the predictions recorded in the history, no replaying needed.
    /// </summary>
    internal static int HitScore(IReadOnlyList<Round> history, int predictorIndex) =>
        history.Count(round => round.Predictions[predictorIndex] == round.Player);

    private Move ChooseMove(IReadOnlyList<Round> history, IReadOnlyList<Move?> predictions)
    {
        if (_random.NextDouble() < _explorationRate)
        {
            return RandomMove();
        }

        Move? prediction = BestPredictorIndex(history) is int best ? predictions[best] : null;
        if (prediction is null)
        {
            return RandomMove();
        }

        IReadOnlyList<Move> counters = Rules.CountersOf(prediction.Value);
        return counters[_random.Next(counters.Count)];
    }

    // On equal scores the earlier predictor wins, which makes the order of the list meaningful:
    // put simple predictors first, they need the least data.
    private int? BestPredictorIndex(IReadOnlyList<Round> history)
    {
        int? best = null;
        int bestScore = -1;

        for (int index = 0; index < _predictors.Count; index++)
        {
            int score = HitScore(history, index);
            if (score > bestScore)
            {
                best = index;
                bestScore = score;
            }
        }

        return best;
    }

    // Recorded predictions are matched to predictors by position, so a history from a computer
    // with other predictors would silently give wrong scores. Failing loudly is safer.
    private void EnsureRecordedByThisConfiguration(IReadOnlyList<Round> history)
    {
        if (history.Any(round => round.Predictions.Count != _predictors.Count))
        {
            throw new ArgumentException(
                $"The history was recorded by a computer with a different set of predictors; expected {_predictors.Count} predictions per round.",
                nameof(history));
        }
    }

    private Move RandomMove() => Moves[_random.Next(Moves.Length)];
}
