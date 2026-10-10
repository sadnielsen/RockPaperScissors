namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// Prediction with expert advice: tracks how well each predictor has been doing and follows
/// the best one. Because it is itself an <see cref="IPredictor"/>, <see cref="PatternStrategy"/>
/// keeps the single place where a prediction is turned into a move.
/// </summary>
public sealed class EnsemblePredictor : IPredictor
{
    private const double CorrectPredictionReward = 1.0;

    private readonly IReadOnlyList<IPredictor> _predictors;
    private readonly double[] _scores;
    private readonly double _decay;

    /// <param name="predictors">
    /// Ordered from simple to complex: on equal scores the earlier one wins, so early in the game,
    /// when complex contexts have seen too little data, the simple one is followed.
    /// </param>
    public EnsemblePredictor(IReadOnlyList<IPredictor> predictors, EnsembleOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(predictors);
        if (predictors.Count == 0)
        {
            throw new ArgumentException("At least one predictor is required.", nameof(predictors));
        }

        var decay = (options ?? new EnsembleOptions()).Decay;
        ArgumentOutOfRangeException.ThrowIfLessThan(decay, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(decay, 1.0);

        _predictors = predictors.ToArray();
        _scores = new double[_predictors.Count];
        _decay = decay;
    }

    public IReadOnlyList<double> Scores => _scores;

    public Move? Predict(IReadOnlyList<Round> history)
    {
        Move? best = null;
        var bestScore = double.NegativeInfinity;

        for (var i = 0; i < _predictors.Count; i++)
        {
            // A predictor without a prediction (too little history for its context) cannot be followed.
            if (_predictors[i].Predict(history) is { } prediction && _scores[i] > bestScore)
            {
                best = prediction;
                bestScore = _scores[i];
            }
        }

        return best;
    }

    public void Learn(IReadOnlyList<Round> historyBefore, Move actual)
    {
        for (var i = 0; i < _predictors.Count; i++)
        {
            // Score what the predictor would have said before it learns the answer.
            var wasCorrect = _predictors[i].Predict(historyBefore) == actual;
            _scores[i] = _scores[i] * _decay + (wasCorrect ? CorrectPredictionReward : 0.0);

            _predictors[i].Learn(historyBefore, actual);
        }
    }
}
