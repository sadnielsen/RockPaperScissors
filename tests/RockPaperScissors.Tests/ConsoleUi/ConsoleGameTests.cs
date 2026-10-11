using RockPaperScissors.Console;
using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.ConsoleUi;

public sealed class ConsoleGameTests
{
    private const string InvalidRounds = "Ongeldig aantal";
    private const string InvalidMove = "Ongeldige zet";

    [Fact]
    public void Run_ValidGame_PlaysAllRoundsAndShowsFinalScore()
    {
        // Rock vs Scissors (win), Rock vs Paper (lose).
        string output = Run("2\nr\nrots\n", new RecordingComputerPlayer(Move.Scissors, Move.Paper));

        Assert.Contains("Jij: rots, computer: schaar. Je wint deze ronde!", output);
        Assert.Contains("Jij: rots, computer: papier. De computer wint deze ronde.", output);
        Assert.Contains("Stand na 2 rondes: gewonnen 1, verloren 1, gelijk 0.", output);
        Assert.Contains("Het spel eindigt gelijk.", output);
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
        var computer = new RecordingComputerPlayer();

        string output = Run($"{invalid}\n1\nr\n", computer);

        Assert.Equal(1, Occurrences(output, InvalidRounds));
        Assert.Single(computer.ReceivedHistories);
    }

    [Fact]
    public void Run_MaxRounds_IsAccepted()
    {
        string output = Run($"{GameSession.MaxRounds}\n", new RecordingComputerPlayer());

        Assert.DoesNotContain(InvalidRounds, output);
        Assert.Contains($"Ronde 1/{GameSession.MaxRounds}", output);
    }

    [Fact]
    public void Run_InvalidMoves_AsksAgainWithoutPlayingRound()
    {
        var computer = new RecordingComputerPlayer();

        string output = Run("1\nx\n\nrock\ns\n", computer);

        Assert.Equal(3, Occurrences(output, InvalidMove));
        Assert.Single(computer.ReceivedHistories);
        Assert.Contains("Stand na 1 rondes", output);
    }

    [Fact]
    public void Run_InputEndsBeforeNumberOfRounds_ExitsCleanly()
    {
        var computer = new RecordingComputerPlayer();

        string output = Run("", computer);

        Assert.Contains("Tot ziens", output);
        Assert.Empty(computer.ReceivedHistories);
    }

    [Fact]
    public void Run_InputEndsDuringGame_ShowsScoreSoFar()
    {
        string output = Run("3\np\n", new RecordingComputerPlayer(Move.Rock));

        Assert.Contains("afgebroken", output);
        Assert.Contains("Stand na 1 rondes: gewonnen 1, verloren 0, gelijk 0.", output);
    }

    [Fact]
    public void Run_PlayerWinsMoreRounds_AnnouncesPlayerAsWinner()
    {
        string output = Run("1\np\n", new RecordingComputerPlayer(Move.Rock));

        Assert.Contains("Jij wint het spel!", output);
    }

    [Fact]
    public void Run_ComputerWinsMoreRounds_AnnouncesComputerAsWinner()
    {
        string output = Run("1\nr\n", new RecordingComputerPlayer(Move.Paper));

        Assert.Contains("De computer wint het spel.", output);
    }

    private static string Run(string input, IComputerPlayer computer)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();

        new ConsoleGame(reader, writer, totalRounds => new GameSession(totalRounds, computer)).Run();

        return writer.ToString();
    }

    private static int Occurrences(string text, string value) =>
        text.Split(value).Length - 1;
}
