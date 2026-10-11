namespace RockPaperScissors.Core;

/// <summary>
/// The computer's move for a round, plus what each of its predictors expected the player to play,
/// in the order of the computer's predictors. Both are decided before the player's move is known.
/// </summary>
public sealed record ComputerDecision(Move Move, IReadOnlyList<Move?> Predictions);
