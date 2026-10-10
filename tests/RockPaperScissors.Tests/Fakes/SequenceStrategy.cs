using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>Plays a fixed sequence of moves (cycling), so tests know the computer's move in advance.</summary>
internal sealed class SequenceStrategy : IStrategy
{
    private readonly Move[] _moves;
    private int _index;

    public SequenceStrategy(params Move[] moves)
    {
        _moves = moves;
    }

    public Move ChooseMove(IReadOnlyList<Round> history) => _moves[_index++ % _moves.Length];
}
