namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// The memory of a context: per key, how often each move followed. It only counts;
/// what the key means is up to the context, what to do with a prediction is up to the strategy.
/// </summary>
public sealed class TransitionTable<TKey> where TKey : struct
{
    // Laplace smoothing: every cell starts at one, so a single observation is not a hard conclusion.
    public const int InitialCount = 1;

    private static readonly Move[] Moves = Enum.GetValues<Move>();

    private readonly Dictionary<TKey, int[]> _counts = new();
    private readonly Random _random;

    public TransitionTable(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        _random = random;
    }

    public void Record(TKey key, Move next)
    {
        if (!Enum.IsDefined(next))
        {
            throw new ArgumentOutOfRangeException(nameof(next), next, "Unknown move.");
        }

        if (!_counts.TryGetValue(key, out var row))
        {
            row = CreateRow();
            _counts.Add(key, row);
        }

        row[(int)next]++;
    }

    public int CountOf(TKey key, Move next) =>
        _counts.TryGetValue(key, out var row) ? row[(int)next] : InitialCount;

    /// <summary>
    /// Returns the move that most often followed <paramref name="key"/>. Ties are broken at random,
    /// so an unseen key gives no preference instead of always favouring the first enum value.
    /// </summary>
    public Move Predict(TKey key)
    {
        var leaders = new List<Move>(Moves.Length);
        var highest = int.MinValue;

        foreach (var move in Moves)
        {
            var count = CountOf(key, move);
            if (count > highest)
            {
                highest = count;
                leaders.Clear();
            }

            if (count == highest)
            {
                leaders.Add(move);
            }
        }

        return leaders[_random.Next(leaders.Count)];
    }

    private static int[] CreateRow()
    {
        var row = new int[Moves.Length];
        Array.Fill(row, InitialCount);
        return row;
    }
}
