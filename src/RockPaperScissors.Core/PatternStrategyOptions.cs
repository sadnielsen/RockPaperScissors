namespace RockPaperScissors.Core;

public sealed record PatternStrategyOptions
{
    public const double DefaultExplorationRate = 0.2;

    /// <summary>
    /// Chance (0 to 1) of playing at random despite having a prediction, so the computer
    /// does not become predictable itself.
    /// </summary>
    public double ExplorationRate { get; init; } = DefaultExplorationRate;
}
