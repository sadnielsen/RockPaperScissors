using RockPaperScissors.Core;

namespace RockPaperScissors.Tests;

public sealed class OpponentsTests
{
    public static TheoryData<string> OpponentNames => new(Opponents.All.Select(option => option.Name));

    [Theory]
    [MemberData(nameof(OpponentNames))]
    public void Create_EachOpponent_ChoosesMoveOnEmptyAndPlayedHistory(string name)
    {
        IComputerPlayer computer = Opponents.All.Single(option => option.Name == name).Create(new Random(42));

        Assert.Contains(computer.ChooseMove([]), Enum.GetValues<Move>());
        Assert.Contains(
            computer.ChooseMove(HistoryTestData.FromPlayerMoves(Move.Rock, Move.Paper, Move.Rock)),
            Enum.GetValues<Move>());
    }

    [Fact]
    public void Create_SameOpponentTwice_ReturnsSeparatePlayers()
    {
        OpponentOption option = Opponents.Default;

        Assert.NotSame(option.Create(new Random(1)), option.Create(new Random(1)));
    }

    [Fact]
    public void Default_IsOneOfAll()
    {
        Assert.Contains(Opponents.Default, Opponents.All);
    }

    [Fact]
    public void All_Names_AreUnique()
    {
        Assert.Equal(Opponents.All.Count, Opponents.All.Select(option => option.Name).Distinct().Count());
    }
}
