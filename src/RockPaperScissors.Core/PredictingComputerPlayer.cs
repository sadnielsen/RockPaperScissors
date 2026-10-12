using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Core;

/// <summary>
/// Plays a counter to what its predictor expects, and sometimes a random move.
/// The tactic lives in the predictor; this class holds the logic every tactic shares, once.
/// Holds no game state: every choice is a function of the history and the random numbers.
/// </summary>
public sealed class PredictingComputerPlayer : IComputerPlayer
{
    /// <summary>Chance of a random move, so the computer itself does not become predictable.</summary>
    public const double DefaultExplorationRate = 0.2;

    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly IPredictor _predictor;
    private readonly Random _random;
    private readonly double _explorationRate;

    public PredictingComputerPlayer(IPredictor predictor, Random random, double explorationRate)
    {
        ArgumentNullException.ThrowIfNull(predictor);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(explorationRate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(explorationRate, 1.0);

        _predictor = predictor;
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

        Move? prediction = _predictor.Predict(history);
        if (prediction is null)
        {
            return RandomMove();
        }

        // With more than three moves a prediction has several counters; any of them wins.
        IReadOnlyList<Move> counters = Rules.CountersOf(prediction.Value);
        return counters[_random.Next(counters.Count)];
    }

    private Move RandomMove() => Moves[_random.Next(Moves.Length)];
}
