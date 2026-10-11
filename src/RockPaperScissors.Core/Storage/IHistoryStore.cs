namespace RockPaperScissors.Core.Storage;

/// <summary>
/// Hook for keeping a player's history between games. Because the history is the only state,
/// storing the rounds is enough: everything else can be recomputed from them.
/// </summary>
public interface IHistoryStore
{
    /// <returns>The stored rounds, or an empty list for an unknown player.</returns>
    IReadOnlyList<Round> Load(string playerId);

    void Save(string playerId, IReadOnlyList<Round> history);
}
