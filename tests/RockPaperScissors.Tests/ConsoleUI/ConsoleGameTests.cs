using RockPaperScissors.ConsoleUI;
using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.ConsoleUI;

public sealed class ConsoleGameTests
{
    private const string InvalidRounds = "Ongeldig aantal";
    private const string InvalidMove = "Ongeldige zet";

    [Fact]
    public void Run_ValidGame_PlaysAllRoundsAndShowsFinalScore()
    {
        // Rock vs Scissors (win), Rock vs Paper (lose).
        string output = Run("2\nr\nrots\n", new FakeRandom(Move.Scissors, Move.Paper));

        Assert.Contains("Jij: rots, computer: schaar. Je wint deze ronde!", output);
        Assert.Contains("Jij: rots, computer: papier. De computer wint deze ronde.", output);
        Assert.Contains("Stand na 2 rondes: gewonnen 1, verloren 1, gelijk 0.", output);
        Assert.Contains("Het spel eindigt gelijk.", output);
    }

    [Fact]
    public void Run_Draw_IsShownAndCountedSeparately()
    {
        string output = Run("1\nr\n", new FakeRandom(Move.Rock));

        Assert.Contains("Jij: rots, computer: rots. Gelijkspel.", output);
        Assert.Contains("Stand na 1 ronde: gewonnen 0, verloren 0, gelijk 1.", output);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1001")]
    [InlineData("2.5")]
    public void Run_InvalidNumberOfRounds_AsksAgain(string invalid)
    {
        string output = Run($"{invalid}\n1\nr\n", new FakeRandom(Move.Rock));

        Assert.Equal(1, Occurrences(output, InvalidRounds));
        Assert.Contains("Stand na 1 ronde:", output);
    }

    [Fact]
    public void Run_MaxRounds_IsAccepted()
    {
        string output = Run($"{GameSession.MaxRounds}\n");

        Assert.DoesNotContain(InvalidRounds, output);
        Assert.Contains($"Ronde 1/{GameSession.MaxRounds}", output);
    }

    [Fact]
    public void Run_InvalidMoves_AsksAgainWithoutPlayingRound()
    {
        string output = Run("1\nx\n\nrock\ns\n", new FakeRandom(Move.Rock));

        Assert.Equal(3, Occurrences(output, InvalidMove));
        Assert.Equal(1, Occurrences(output, "Jij: "));
        Assert.Contains("Stand na 1 ronde:", output);
    }

    [Fact]
    public void Run_InputEndsBeforeNumberOfRounds_ExitsCleanly()
    {
        string output = Run("");

        Assert.Contains("Tot ziens", output);
        Assert.DoesNotContain("Stand na", output);
    }

    [Fact]
    public void Run_InputEndsDuringGame_ShowsScoreSoFar()
    {
        string output = Run("3\np\n", new FakeRandom(Move.Rock));

        Assert.Contains("afgebroken", output);
        Assert.Contains("Stand na 1 ronde: gewonnen 1, verloren 0, gelijk 0.", output);
    }

    [Fact]
    public void Run_PlayerWinsMoreRounds_AnnouncesPlayerAsWinner()
    {
        string output = Run("1\np\n", new FakeRandom(Move.Rock));

        Assert.Contains("Jij wint het spel!", output);
    }

    [Fact]
    public void Run_ComputerWinsMoreRounds_AnnouncesComputerAsWinner()
    {
        string output = Run("1\nr\n", new FakeRandom(Move.Paper));

        Assert.Contains("De computer wint het spel.", output);
    }

    // The fake random scripts the computer's moves, so the game is fully predictable.
    // Without moves, any round played makes the fake throw.
    private static string Run(string input, FakeRandom? random = null)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();

        new ConsoleGame(reader, writer, random ?? new FakeRandom()).Run();

        return writer.ToString();
    }

    private static int Occurrences(string text, string value) =>
        text.Split(value).Length - 1;
}
