namespace RockPaperScissors.Core;

public interface IComputerPlayer
{
    /// <param name="history">Only completed rounds; never the player's move for the current round.</param>
    Move ChooseMove(IReadOnlyList<Round> history);
}
