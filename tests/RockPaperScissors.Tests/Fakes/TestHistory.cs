using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>Builds histories for tests, with outcomes resolved by the real rules so they are always consistent.</summary>
internal static class TestHistory
{
    private static readonly Rules Rules = new();

    public static IReadOnlyList<Round> Empty { get; } = Array.Empty<Round>();

    public static IReadOnlyList<Round> Of(params (Move Player, Move Computer)[] moves) =>
        moves.Select(m => new Round(m.Player, m.Computer, Rules.Resolve(m.Player, m.Computer))).ToArray();
}
