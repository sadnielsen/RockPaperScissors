using RockPaperScissors.Core.Prediction;

namespace RockPaperScissors.Core;

/// <summary>
/// The decision maker: asks a predictor for the player's next move and plays its counter,
/// with some random play mixed in.
/// </summary>
public sealed class PatternStrategy : IStrategy
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly IPredictor _predictor;
    private readonly Rules _rules;
    private readonly Random _random;
    private readonly double _explorationRate;
    private int _processedRounds;

    public PatternStrategy(IPredictor predictor, Rules rules, Random random, PatternStrategyOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(predictor);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(random);

        var explorationRate = (options ?? new PatternStrategyOptions()).ExplorationRate;
        ArgumentOutOfRangeException.ThrowIfLessThan(explorationRate, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(explorationRate, 1.0);

        _predictor = predictor;
        _rules = rules;
        _random = random;
        _explorationRate = explorationRate;
    }

    public Move ChooseMove(IReadOnlyList<Round> history)
    {
        LearnNewRounds(history);

        var prediction = _predictor.Predict(history);
        if (prediction is null || _random.NextDouble() < _explorationRate)
        {
            return Moves[_random.Next(Moves.Length)];
        }

        return _rules.CounterOf(prediction.Value);
    }

    // Learning is derived from the history itself: every round not seen before is replayed once,
    // with exactly the history that preceded it.
    private void LearnNewRounds(IReadOnlyList<Round> history)
    {
        if (history.Count < _processedRounds)
        {
            throw new InvalidOperationException(
                $"History shrank from {_processedRounds} to {history.Count} rounds; it may only grow.");
        }

        for (var i = _processedRounds; i < history.Count; i++)
        {
            _predictor.Learn(new HistoryPrefix(history, i), history[i].Player);
        }

        _processedRounds = history.Count;
    }
}
