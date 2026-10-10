using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>Records what the strategy was shown on each call, to prove it never sees the current move.</summary>
internal sealed class SpyStrategy : IStrategy
{
    private readonly List<Round[]> _observedHistories = new();

    public IReadOnlyList<Round[]> ObservedHistories => _observedHistories;

    public IReadOnlyList<Round>? LastHistoryReference { get; private set; }

    public Move ChooseMove(IReadOnlyList<Round> history)
    {
        LastHistoryReference = history;
        _observedHistories.Add(history.ToArray());
        return Move.Rock;
    }
}
