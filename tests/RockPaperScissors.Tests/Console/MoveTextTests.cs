using RockPaperScissors.Console;
using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Console;

public sealed class MoveTextTests
{
    [Theory]
    [InlineData("r", Move.Rock)]
    [InlineData("rots", Move.Rock)]
    [InlineData("R", Move.Rock)]
    [InlineData("  Rots ", Move.Rock)]
    [InlineData("p", Move.Paper)]
    [InlineData("PAPIER", Move.Paper)]
    [InlineData("s", Move.Scissors)]
    [InlineData("Schaar", Move.Scissors)]
    public void TryParse_ValidInput_ReturnsMove(string input, Move expected)
    {
        Assert.True(MoveText.TryParse(input, out var move));
        Assert.Equal(expected, move);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("x")]
    [InlineData("steen")]
    [InlineData("rotss")]
    [InlineData("r p")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(MoveText.TryParse(input, out _));
    }

    [Theory]
    [InlineData(Move.Rock, "rots")]
    [InlineData(Move.Paper, "papier")]
    [InlineData(Move.Scissors, "schaar")]
    public void NameOf_Move_ReturnsDutchName(Move move, string expected)
    {
        Assert.Equal(expected, MoveText.NameOf(move));
    }

    [Theory]
    [InlineData(Move.Rock)]
    [InlineData(Move.Paper)]
    [InlineData(Move.Scissors)]
    public void NameOf_AnyMove_ParsesBackToSameMove(Move move)
    {
        Assert.True(MoveText.TryParse(MoveText.NameOf(move), out var parsed));
        Assert.Equal(move, parsed);
    }
}
