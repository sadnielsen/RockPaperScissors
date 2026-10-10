namespace RockPaperScissors.Core;

/// <summary>Running score from the player's point of view.</summary>
public readonly record struct Score(int Wins, int Losses, int Draws)
{
    public Score Add(Outcome outcome) => outcome switch
    {
        Outcome.Win => this with { Wins = Wins + 1 },
        Outcome.Lose => this with { Losses = Losses + 1 },
        Outcome.Draw => this with { Draws = Draws + 1 },
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown outcome."),
    };
}
