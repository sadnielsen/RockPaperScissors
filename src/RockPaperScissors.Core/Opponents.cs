using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Core;

/// <summary>
/// The opponents a player can choose from, in one place so Console and Web offer the same choice.
/// Adding an opponent is adding a line here.
/// </summary>
public static class Opponents
{
    // The names are shown to the player as they are, which is why they are Dutch.
    private static readonly OpponentOption LastMoveAndOutcome = Predicting(
        "Zet + uitkomst", "lastig: straft win-stay/lose-shift af, het meest voorkomende menselijke patroon",
        new LastMoveAndOutcomePredictor());

    public static IReadOnlyList<OpponentOption> All { get; } =
    [
        new("Random", "makkelijk, geen patroon", random => new RandomComputerPlayer(random)),
        Predicting("Frequentie", "straft een voorkeurszet af", new FrequencyPredictor()),
        Predicting("Vorige zet", "straft vaste reeksen af", new LastMovePredictor()),
        Predicting("Laatste twee zetten", "straft langere reeksen af", new LastTwoMovesPredictor()),
        LastMoveAndOutcome,
    ];

    /// <summary>The hardest opponent: it targets the pattern humans fall into most.</summary>
    public static OpponentOption Default { get; } = LastMoveAndOutcome;

    // Predictors hold no state, so one instance can safely serve every game.
    private static OpponentOption Predicting(string name, string description, IPredictor predictor) =>
        new(name, description, random =>
            new PredictingComputerPlayer(predictor, random, PredictingComputerPlayer.DefaultExplorationRate));
}
