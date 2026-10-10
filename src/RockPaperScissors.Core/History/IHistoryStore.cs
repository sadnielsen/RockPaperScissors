namespace RockPaperScissors.Core.History;

/// <summary>
/// Hook for keeping a player's history across sessions. Only an in-memory version exists;
/// see the README for why persistent storage was deliberately left out.
/// </summary>
public interface IHistoryStore
{
    /// <returns>The stored rounds, or an empty list for an unknown player.</returns>
    IReadOnlyList<Round> Load(string playerId);

    void Save(string playerId, IReadOnlyList<Round> history);
}
