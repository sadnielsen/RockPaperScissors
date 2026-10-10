namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// A hypothesis about what drives the player's next move: it turns the history into a key.
/// It makes no prediction itself; counting and deciding happen elsewhere.
/// </summary>
/// <typeparam name="TKey">A value type, so keys compare by content and work as dictionary keys.</typeparam>
public interface IContext<TKey> where TKey : struct
{
    /// <returns>The key for the current situation, or <c>null</c> when there is too little history.</returns>
    TKey? GetKey(IReadOnlyList<Round> history);
}
