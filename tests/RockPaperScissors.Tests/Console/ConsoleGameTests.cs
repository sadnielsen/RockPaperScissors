using RockPaperScissors.Console;
using RockPaperScissors.Core;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests.Console;

public sealed class ConsoleGameTests
{
    private const string InvalidRoundCountMessage = "Ongeldig aantal";
    private const string InvalidMoveMessage = "Ongeldige zet";

    private readonly Rules _rules = new();
    private readonly StringWriter _output = new();
    private int? _requestedRounds;

    [Fact]
    public void Run_CompleteGame_PrintsEachRoundAndFinalScore()
    {
        // Computer always plays rock: paper wins, rock draws, scissors loses.
        Run(new SequenceStrategy(Move.Rock), "3", "p", "r", "s");

        var output = _output.ToString();
        Assert.Contains("Jij: papier, computer: rots. Jij wint deze ronde.", output);
        Assert.Contains("Eindstand na 3 rondes:", output);
        Assert.Contains("1 gewonnen, 1 verloren, 1 gelijk.", output);
        Assert.Contains("Het is gelijkspel.", output);
    }

    [Fact]
    public void Run_InvalidRoundCounts_AsksAgainUntilValid()
    {
        Run(new SequenceStrategy(Move.Rock), "", "abc", "0", "-3", "1001", "2.5", "1", "r");

        Assert.Equal(6, CountOccurrences(_output.ToString(), InvalidRoundCountMessage));
        Assert.Equal(1, _requestedRounds);
    }

    [Fact]
    public void Run_InvalidMoves_AsksAgainUntilValid()
    {
        Run(new SequenceStrategy(Move.Rock), "1", "x", "steen", "", "ROTS");

        var output = _output.ToString();
        Assert.Equal(3, CountOccurrences(output, InvalidMoveMessage));
        Assert.Contains("Eindstand na 1 rondes:", output);
    }

    [Fact]
    public void Run_EndOfInputBeforeRoundCount_ExitsWithoutStartingGame()
    {
        Run(new SequenceStrategy(Move.Rock));

        Assert.Contains("Tot ziens", _output.ToString());
        Assert.Null(_requestedRounds);
    }

    [Fact]
    public void Run_EndOfInputDuringGame_ExitsWithScoreSoFar()
    {
        Run(new SequenceStrategy(Move.Rock), "3", "p");

        var output = _output.ToString();
        Assert.Contains("Spel afgebroken na 1 van 3 rondes.", output);
        Assert.Contains("1 gewonnen, 0 verloren, 0 gelijk.", output);
        Assert.DoesNotContain("Eindstand", output);
    }

    [Fact]
    public void Run_EveryRound_ComputerChoosesBeforePlayerInputIsRead()
    {
        var log = new List<string>();
        var game = new ConsoleGame(
            new LoggingReader(log, "3", "r", "p", "s"),
            _output,
            rounds => new GameSession(rounds, new LoggingStrategy(log), _rules));

        game.Run();

        Assert.Equal(
            new[] { "read:3", "choose", "read:r", "choose", "read:p", "choose", "read:s" },
            log);
    }

    private void Run(IStrategy strategy, params string[] lines)
    {
        var input = new StringReader(string.Join(Environment.NewLine, lines));
        var game = new ConsoleGame(input, _output, rounds =>
        {
            _requestedRounds = rounds;
            return new GameSession(rounds, strategy, _rules);
        });

        game.Run();
    }

    private static int CountOccurrences(string text, string value) =>
        (text.Length - text.Replace(value, string.Empty, StringComparison.Ordinal).Length) / value.Length;
}
