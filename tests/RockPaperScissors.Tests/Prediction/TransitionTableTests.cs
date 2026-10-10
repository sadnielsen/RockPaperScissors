using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;

namespace RockPaperScissors.Tests.Prediction;

public sealed class TransitionTableTests
{
    private const int Seed = 42;
    private const int SeedsToTry = 50;

    private static readonly LastMoveKey RockKey = new(Move.Rock);
    private static readonly LastMoveKey PaperKey = new(Move.Paper);

    [Fact]
    public void CountOf_NewTable_StartsAtLaplaceCountForEveryMove()
    {
        var table = new TransitionTable<LastMoveKey>(new Random(Seed));

        Assert.All(Enum.GetValues<Move>(), move =>
            Assert.Equal(TransitionTable<LastMoveKey>.InitialCount, table.CountOf(RockKey, move)));
    }

    [Fact]
    public void Record_Move_IncrementsOnlyThatCellForThatKey()
    {
        var table = new TransitionTable<LastMoveKey>(new Random(Seed));

        table.Record(RockKey, Move.Paper);
        table.Record(RockKey, Move.Paper);

        Assert.Equal(3, table.CountOf(RockKey, Move.Paper));
        Assert.Equal(1, table.CountOf(RockKey, Move.Rock));
        Assert.Equal(1, table.CountOf(RockKey, Move.Scissors));
        Assert.Equal(1, table.CountOf(PaperKey, Move.Paper));
    }

    [Fact]
    public void Record_UndefinedMove_ThrowsArgumentOutOfRangeException()
    {
        var table = new TransitionTable<LastMoveKey>(new Random(Seed));

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Record(RockKey, (Move)42));
    }

    [Fact]
    public void Predict_OneMoveCountedMost_ReturnsThatMove()
    {
        var table = new TransitionTable<LastMoveKey>(new Random(Seed));
        table.Record(RockKey, Move.Scissors);
        table.Record(RockKey, Move.Scissors);
        table.Record(RockKey, Move.Paper);

        Assert.Equal(Move.Scissors, table.Predict(RockKey));
    }

    [Fact]
    public void Predict_UnseenKey_CanReturnEveryMove()
    {
        var predictions = Enumerable.Range(0, SeedsToTry)
            .Select(seed => new TransitionTable<LastMoveKey>(new Random(seed)).Predict(RockKey))
            .ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), predictions);
    }

    [Fact]
    public void Predict_TwoMovesTied_ReturnsOnlyTheLeaders()
    {
        var predictions = Enumerable.Range(0, SeedsToTry)
            .Select(seed =>
            {
                var table = new TransitionTable<LastMoveKey>(new Random(seed));
                table.Record(RockKey, Move.Rock);
                table.Record(RockKey, Move.Paper);
                return table.Predict(RockKey);
            })
            .ToHashSet();

        Assert.Equal(new HashSet<Move> { Move.Rock, Move.Paper }, predictions);
    }

    [Fact]
    public void Predict_UnseenKey_DoesNotAddRow()
    {
        var table = new TransitionTable<LastMoveKey>(new Random(Seed));

        table.Predict(PaperKey);
        table.Record(RockKey, Move.Rock);

        Assert.Equal(TransitionTable<LastMoveKey>.InitialCount, table.CountOf(PaperKey, Move.Rock));
    }
}
