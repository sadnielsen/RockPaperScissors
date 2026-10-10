using System.Collections.Concurrent;

namespace RockPaperScissors.Core.History;

public sealed class InMemoryHistoryStore : IHistoryStore
{
    // Concurrent because a web host serves several players at once.
    private readonly ConcurrentDictionary<string, Round[]> _histories = new(StringComparer.Ordinal);

    public IReadOnlyList<Round> Load(string playerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        return _histories.TryGetValue(playerId, out var history) ? history : Array.Empty<Round>();
    }

    public void Save(string playerId, IReadOnlyList<Round> history)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        ArgumentNullException.ThrowIfNull(history);

        // Store a copy: the caller's list (e.g. a live session history) may keep changing.
        _histories[playerId] = history.ToArray();
    }
}
