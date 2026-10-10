namespace RockPaperScissors.Core;

/// <summary>
/// Chooses the computer's move. A strategy only ever sees completed rounds, never the
/// player's current move, which keeps the computer honest by construction.
/// </summary>
public interface IStrategy
{
    Move ChooseMove(IReadOnlyList<Round> history);
}
