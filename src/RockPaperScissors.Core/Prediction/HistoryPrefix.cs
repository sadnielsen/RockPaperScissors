using System.Collections;

namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// A read-only view on the first rounds of a history, so replaying the history round by round
/// does not copy it every time.
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
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return _source[index];
        }
    }

    public IEnumerator<Round> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return _source[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
