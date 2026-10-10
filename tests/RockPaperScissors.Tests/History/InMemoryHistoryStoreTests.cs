using RockPaperScissors.Core;
using RockPaperScissors.Core.History;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.History;

public sealed class InMemoryHistoryStoreTests
{
    private const string PlayerId = "player-1";

    private readonly InMemoryHistoryStore _store = new();

    [Fact]
    public void Load_UnknownPlayer_ReturnsEmptyHistory()
    {
        Assert.Empty(_store.Load(PlayerId));
    }

    [Fact]
    public void Load_AfterSave_ReturnsSavedRounds()
    {
        var history = TestHistory.Of((Move.Rock, Move.Paper), (Move.Scissors, Move.Paper));

        _store.Save(PlayerId, history);

        Assert.Equal(history, _store.Load(PlayerId));
    }

    [Fact]
    public void Load_OtherPlayer_DoesNotReturnSomeoneElsesHistory()
    {
        _store.Save(PlayerId, TestHistory.Of((Move.Rock, Move.Paper)));

        Assert.Empty(_store.Load("player-2"));
    }

    [Fact]
    public void Save_SourceListChangesAfterwards_StoredHistoryStaysTheSame()
    {
        var live = new List<Round>(TestHistory.Of((Move.Rock, Move.Paper)));
        _store.Save(PlayerId, live);

        live.Add(new Round(Move.Paper, Move.Paper, Outcome.Draw));

        Assert.Single(_store.Load(PlayerId));
    }

    [Fact]
    public void Save_SamePlayerTwice_KeepsLatestHistory()
    {
        _store.Save(PlayerId, TestHistory.Of((Move.Rock, Move.Paper)));
        var latest = TestHistory.Of((Move.Paper, Move.Paper), (Move.Paper, Move.Rock));

        _store.Save(PlayerId, latest);

        Assert.Equal(latest, _store.Load(PlayerId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Load_BlankPlayerId_ThrowsArgumentException(string playerId)
    {
        Assert.Throws<ArgumentException>(() => _store.Load(playerId));
    }
}
