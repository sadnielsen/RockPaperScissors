namespace RockPaperScissors.Core.Prediction;

public readonly record struct Last2MovesKey(Move SecondToLastPlayerMove, Move LastPlayerMove);
