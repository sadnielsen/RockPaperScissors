using System.Collections;

namespace RockPaperScissors.Core;

/// <summary>
/// A read-only view of the first rounds of a history, without copying.
/// Used when replaying a history, so a predictor scoring round i can only see the rounds before it.
/// </summary>
internal sealed class HistoryPrefix : IReadOnlyList<Round>
{
    private readonly IReadOnlyList<Round> _source;

    public HistoryPrefix(IReadOnlyList<Round> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, source.Count);

        _source = source;
        Count = count;
    }

    public int Count { get; }

    public Round this[int index]
    {
        get
        {
            // Bounds are checked against the prefix, not the source: looking past it would be cheating.
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return _source[index];
        }
    }

    public IEnumerator<Round> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _source[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
