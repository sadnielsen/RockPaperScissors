using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>
/// A <see cref="Random"/> that makes the computer play scripted moves, so a game is fully predictable in tests.
/// </summary>
internal sealed class FakeRandom(params Move[] computerMoves) : Random
{
    private readonly Queue<Move> _moves = new(computerMoves);

    // GameSession picks Enum.GetValues<Move>()[Next(count)], so the index of a move selects it.
    // Running out throws instead of falling back to a default, so a test that plays more rounds
    // than it scripted fails loudly.
    public override int Next(int maxValue) =>
        _moves.TryDequeue(out Move move)
            ? Array.IndexOf(Enum.GetValues<Move>(), move)
            : throw new InvalidOperationException("No scripted computer move left.");

    // Without these, a GameSession that switched to another overload would silently get
    // seed-based numbers from the base class instead of the scripted moves.
    public override int Next() => throw NotScripted();

    public override int Next(int minValue, int maxValue) => throw NotScripted();

    public override double NextDouble() => throw NotScripted();

    protected override double Sample() => throw NotScripted();

    private static NotSupportedException NotScripted() =>
        new("FakeRandom only scripts Next(int); GameSession must use that overload.");
}
