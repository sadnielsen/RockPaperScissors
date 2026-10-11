using RockPaperScissors.Core;
using RockPaperScissors.Web;

namespace RockPaperScissors.Tests.WebUi;

public sealed class MoveNamesTests
{
    // Fails as soon as a move is added without a display name, instead of crashing the page.
    [Fact]
    public void NameOf_EveryMove_HasDistinctName()
    {
        var names = Enum.GetValues<Move>().Select(MoveNames.NameOf).ToList();

        Assert.All(names, name => Assert.False(string.IsNullOrWhiteSpace(name)));
        Assert.Equal(names.Count, names.Distinct().Count());
    }
}
