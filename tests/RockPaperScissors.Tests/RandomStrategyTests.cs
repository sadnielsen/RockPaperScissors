using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class RandomStrategyTests
{
    private const int Seed = 42;
    private const int Draws = 300;

    private static readonly IReadOnlyList<Round> EmptyHistory = Array.Empty<Round>();

    [Fact]
    public void ChooseMove_SameSeed_ReturnsSameSequence()
    {
        var first = new RandomStrategy(new Random(Seed));
        var second = new RandomStrategy(new Random(Seed));

        var firstMoves = Enumerable.Range(0, Draws).Select(_ => first.ChooseMove(EmptyHistory));
        var secondMoves = Enumerable.Range(0, Draws).Select(_ => second.ChooseMove(EmptyHistory));

        Assert.Equal(firstMoves, secondMoves);
    }

    [Fact]
    public void ChooseMove_ManyDraws_ReturnsEveryMoveRoughlyEquallyOften()
    {
        var strategy = new RandomStrategy(new Random(Seed));

        var counts = Enumerable.Range(0, Draws)
            .Select(_ => strategy.ChooseMove(EmptyHistory))
            .GroupBy(move => move)
            .ToDictionary(group => group.Key, group => group.Count());

        // Deterministic thanks to the seed; the bounds only guard against a skewed implementation.
        var expectedPerMove = Draws / Enum.GetValues<Move>().Length;
        Assert.All(Enum.GetValues<Move>(), move =>
            Assert.InRange(counts.GetValueOrDefault(move), expectedPerMove * 0.7, expectedPerMove * 1.3));
    }

    [Fact]
    public void ChooseMove_DifferentHistory_DoesNotAffectChoice()
    {
        var withoutHistory = new RandomStrategy(new Random(Seed));
        var withHistory = new RandomStrategy(new Random(Seed));
        var history = new[] { new Round(Move.Rock, Move.Paper, Outcome.Lose) };

        Assert.Equal(withoutHistory.ChooseMove(EmptyHistory), withHistory.ChooseMove(history));
    }

    [Fact]
    public void Constructor_NullRandom_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RandomStrategy(null!));
    }
}
