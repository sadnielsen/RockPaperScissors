namespace RockPaperScissors.Core.Prediction;

/// <summary>
/// Couples one context to its own table. Every context gets a separate table,
/// so keys of different contexts are never mixed.
/// </summary>
public sealed class Predictor<TKey> : IPredictor where TKey : struct
{
    private readonly IContext<TKey> _context;
    private readonly TransitionTable<TKey> _table;

    public Predictor(IContext<TKey> context, TransitionTable<TKey> table)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(table);
        _context = context;
        _table = table;
    }

    public Move? Predict(IReadOnlyList<Round> history) =>
        _context.GetKey(history) is { } key ? _table.Predict(key) : null;

    public void Learn(IReadOnlyList<Round> historyBefore, Move actual)
    {
        // Without a key there was no situation to learn from yet (e.g. the first round).
        if (_context.GetKey(historyBefore) is { } key)
        {
            _table.Record(key, actual);
        }
    }
}
