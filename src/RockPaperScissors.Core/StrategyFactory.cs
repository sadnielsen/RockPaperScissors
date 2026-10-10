using RockPaperScissors.Core.Prediction;

namespace RockPaperScissors.Core;

/// <summary>
/// Assembles the default computer opponent, so the console and the web compose it identically.
/// </summary>
public static class StrategyFactory
{
    public static IStrategy CreateDefault(
        Random random,
        Rules rules,
        PatternStrategyOptions? patternOptions = null,
        EnsembleOptions? ensembleOptions = null)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(rules);

        // Simple to complex: on equal scores the ensemble follows the earliest, which has the most data early on.
        var predictors = new IPredictor[]
        {
            new Predictor<LastMoveKey>(new LastMoveContext(), new TransitionTable<LastMoveKey>(random)),
            new Predictor<LastMoveWithOutcomeKey>(new LastMoveWithOutcomeContext(), new TransitionTable<LastMoveWithOutcomeKey>(random)),
            new Predictor<Last2MovesKey>(new Last2MovesContext(), new TransitionTable<Last2MovesKey>(random)),
        };

        return new PatternStrategy(new EnsemblePredictor(predictors, ensembleOptions), rules, random, patternOptions);
    }
}
