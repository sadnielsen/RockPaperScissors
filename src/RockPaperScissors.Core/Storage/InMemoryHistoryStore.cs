namespace RockPaperScissors.Core.Storage;

/// <summary>Keeps histories for the lifetime of the process. Persistent storage is deliberately out of scope.</summary>
public sealed class InMemoryHistoryStore : IHistoryStore
{
    private readonly Dictionary<string, IReadOnlyList<Round>> _histories = [];

    public IReadOnlyList<Round> Load(string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);
        return _histories.TryGetValue(playerId, out IReadOnlyList<Round>? history) ? history : [];
    }

    public void Save(string playerId, IReadOnlyList<Round> history)
    {
        ArgumentNullException.ThrowIfNull(playerId);
        ArgumentNullException.ThrowIfNull(history);

        // A copy, so later changes to the caller's list cannot alter what was saved.
        _histories[playerId] = history.ToList().AsReadOnly();
    }
}
