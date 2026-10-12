using RockPaperScissors.ConsoleUI;
using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.ConsoleUI;

public sealed class MoveTextTests
{
    [Theory]
    [InlineData("r", Move.Rock)]
    [InlineData("rots", Move.Rock)]
    [InlineData("p", Move.Paper)]
    [InlineData("papier", Move.Paper)]
    [InlineData("s", Move.Scissors)]
    [InlineData("schaar", Move.Scissors)]
    [InlineData("ROTS", Move.Rock)]
    [InlineData("  Papier ", Move.Paper)]
    public void Parse_ValidInput_ReturnsMove(string input, Move expected)
    {
        Assert.Equal(expected, MoveText.Parse(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("x")]
    [InlineData("rock")]
    [InlineData("ro")]
    public void Parse_InvalidInput_ReturnsNull(string? input)
    {
        Assert.Null(MoveText.Parse(input));
    }

    // Fails as soon as a move is added without a translation, instead of crashing at runtime.
    [Fact]
    public void NameOf_EveryMove_HasNameThatParsesBack()
    {
        foreach (Move move in Enum.GetValues<Move>())
        {
            Assert.Equal(move, MoveText.Parse(MoveText.NameOf(move)));
        }
    }
}
