namespace RockPaperScissors.Core.Predictors;

/// <summary>
/// Answers the question most predictors ask: "what did the player play after earlier situations like this one?"
/// </summary>
internal static class PatternCounter
{
    /// <summary>
    /// Looks at every earlier round i (i ≥ 1). When the situation before round i matches the current one,
    /// the player's move in round i is counted.
    /// </summary>
    /// <returns>The most counted move, or null when nothing matched or the top count is shared.</returns>
    public static Move? MostFrequentNext(IReadOnlyList<Round> history, Func<int, bool> matchesSituationBefore)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(matchesSituationBefore);

        IEnumerable<Move> followingMoves = Enumerable.Range(1, Math.Max(0, history.Count - 1))
            .Where(matchesSituationBefore)
            .Select(i => history[i].Player);

        return MostFrequent(followingMoves);
    }

    /// <returns>The move that occurs most often, or null when there are none or the top count is shared.</returns>
    public static Move? MostFrequent(IEnumerable<Move> moves)
    {
        ArgumentNullException.ThrowIfNull(moves);

        var counts = moves
            .GroupBy(move => move)
            .Select(group => (Move: group.Key, Count: group.Count()))
            .OrderByDescending(entry => entry.Count)
            .Take(2)
            .ToList();

        if (counts.Count == 0)
        {
            return null;
        }

        // A shared lead is no clear signal; guessing would only add noise.
        if (counts.Count > 1 && counts[0].Count == counts[1].Count)
        {
            return null;
        }

        return counts[0].Move;
    }
}
