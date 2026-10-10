namespace RockPaperScissors.Core.Prediction;

public sealed record EnsembleOptions
{
    public const double DefaultDecay = 0.9;

    /// <summary>
    /// Factor (0 to 1) every score is multiplied by after each round. Lower values forget faster,
    /// so the ensemble switches sooner when the player changes tactics; 1 means never forget.
    /// </summary>
    public double Decay { get; init; } = DefaultDecay;
}
