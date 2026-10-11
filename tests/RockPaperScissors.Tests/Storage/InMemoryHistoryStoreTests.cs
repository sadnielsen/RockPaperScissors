using RockPaperScissors.Core;
using RockPaperScissors.Core.Storage;

namespace RockPaperScissors.Tests.Storage;

public sealed class InMemoryHistoryStoreTests
{
    private readonly InMemoryHistoryStore _store = new();

    [Fact]
    public void Load_UnknownPlayer_ReturnsEmptyHistory()
    {
        Assert.Empty(_store.Load("unknown"));
    }

    [Fact]
    public void Load_AfterSave_ReturnsSavedRounds()
    {
        var history = HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper);

        _store.Save("alice", history);

        Assert.Equal(history, _store.Load("alice"));
    }

    [Fact]
    public void Load_AfterSave_KeepsRecordedPredictions()
    {
        IReadOnlyList<Round> history = [new Round(Move.Rock, Move.Paper, [Move.Scissors, null])];

        _store.Save("alice", history);

        Assert.Equal([Move.Scissors, null], _store.Load("alice")[0].Predictions);
    }

    [Fact]
    public void Load_OtherPlayer_DoesNotSeeHistory()
    {
        _store.Save("alice", HistoryTestData.FromPlayerMoves(Move.Rock));

        Assert.Empty(_store.Load("bob"));
    }

    [Fact]
    public void Save_SourceChangedAfterwards_StoredHistoryIsUnchanged()
    {
        var history = new List<Round> { new(Move.Rock, Move.Paper) };

        _store.Save("alice", history);
        history.Add(new Round(Move.Scissors, Move.Rock));

        Assert.Single(_store.Load("alice"));
    }

    [Fact]
    public void Save_SamePlayerTwice_KeepsLatestHistory()
    {
        _store.Save("alice", HistoryTestData.FromPlayerMoves(Move.Rock));
        var latest = HistoryTestData.FromPlayerMoves(Move.Paper, Move.Scissors);

        _store.Save("alice", latest);

        Assert.Equal(latest, _store.Load("alice"));
    }
}
