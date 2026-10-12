using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class RandomComputerPlayerTests
{
    [Fact]
    public void ChooseMove_AnyHistory_PicksFromAllMoves()
    {
        var random = new FakeRandom(ints: 2);
        var computer = new RandomComputerPlayer(random);

        Move move = computer.ChooseMove(HistoryTestData.FromPlayerMoves(Move.Rock));

        Assert.Equal(Enum.GetValues<Move>()[2], move);
        Assert.Equal([Enum.GetValues<Move>().Length], random.RequestedMaxValues);
    }

    [Fact]
    public void ChooseMove_SeededRandom_PlaysEveryMove()
    {
        var computer = new RandomComputerPlayer(new Random(42));

        var moves = Enumerable.Range(0, 100).Select(_ => computer.ChooseMove([])).ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), moves);
    }
}
