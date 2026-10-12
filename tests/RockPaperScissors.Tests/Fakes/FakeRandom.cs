using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>
/// A <see cref="Random"/> that makes the computer play scripted moves (Rock once exhausted),
/// so a game is fully predictable in tests.
/// </summary>
internal sealed class FakeRandom(params Move[] computerMoves) : Random
{
    private readonly Queue<Move> _moves = new(computerMoves);

    // GameSession picks Enum.GetValues<Move>()[Next(count)], so the index of a move selects it.
    public override int Next(int maxValue) =>
        _moves.TryDequeue(out Move move) ? Array.IndexOf(Enum.GetValues<Move>(), move) : 0;
}
