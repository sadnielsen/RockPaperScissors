using System.Diagnostics;
using System.Globalization;
using RockPaperScissors.Core;

namespace RockPaperScissors.ConsoleUI;

/// <summary>
/// The console loop: reads input, drives a <see cref="GameSession"/>, shows the results.
/// Works on a reader and writer instead of the real console, so the whole flow is testable.
/// </summary>
public sealed class ConsoleGame
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Random _random;

    /// <param name="random">Passed on to the session for the computer's moves.</param>
    public ConsoleGame(TextReader input, TextWriter output, Random random)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(random);

        _input = input;
        _output = output;
        _random = random;
    }

    public void Run()
    {
        _output.WriteLine("Rots, papier, schaar");

        int? totalRounds = ReadTotalRounds();
        if (totalRounds is null)
        {
            _output.WriteLine("Geen invoer meer. Tot ziens!");
            return;
        }

        var session = new GameSession(totalRounds.Value, _random);

        while (!session.IsFinished)
        {
            Move? move = ReadMove(session.History.Count + 1, session.TotalRounds);
            if (move is null)
            {
                _output.WriteLine("Geen invoer meer, het spel is afgebroken.");
                WriteScore(session);
                return;
            }

            WriteRound(session.PlayRound(move.Value));
        }

        WriteScore(session);
        WriteWinner(session);
    }

    /// <returns>A valid number of rounds, or null when the input has ended.</returns>
    private int? ReadTotalRounds()
    {
        while (true)
        {
            _output.Write($"Hoeveel rondes wil je spelen ({GameSession.MinRounds}-{GameSession.MaxRounds})? ");
            string? line = _input.ReadLine();
            if (line is null)
            {
                return null;
            }

            if (int.TryParse(line.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int rounds)
                && GameSession.IsValidTotalRounds(rounds))
            {
                return rounds;
            }

            _output.WriteLine(
                $"Ongeldig aantal. Kies een heel getal van {GameSession.MinRounds} tot en met {GameSession.MaxRounds}.");
        }
    }

    /// <returns>A valid move, or null when the input has ended.</returns>
    private Move? ReadMove(int roundNumber, int totalRounds)
    {
        while (true)
        {
            _output.Write($"Ronde {roundNumber}/{totalRounds}, kies je zet ({MoveText.Choices()}): ");
            string? line = _input.ReadLine();
            if (line is null)
            {
                return null;
            }

            Move? move = MoveText.Parse(line);
            if (move is not null)
            {
                return move;
            }

            _output.WriteLine($"Ongeldige zet. Kies uit: {MoveText.Choices()}.");
        }
    }

    private void WriteRound(Round round)
    {
        string result = round.Outcome switch
        {
            Outcome.Win => "Je wint deze ronde!",
            Outcome.Lose => "De computer wint deze ronde.",
            Outcome.Draw => "Gelijkspel.",
            _ => throw new UnreachableException(),
        };

        _output.WriteLine($"Jij: {MoveText.NameOf(round.Player)}, computer: {MoveText.NameOf(round.Computer)}. {result}");
    }

    private void WriteScore(GameSession session) =>
        _output.WriteLine(
            $"Stand na {RoundsText(session.History.Count)}: gewonnen {session.Wins}, verloren {session.Losses}, gelijk {session.Draws}.");

    private static string RoundsText(int count) => count == 1 ? "1 ronde" : $"{count} rondes";

    private void WriteWinner(GameSession session)
    {
        string result = session.Result switch
        {
            Outcome.Win => "Jij wint het spel!",
            Outcome.Lose => "De computer wint het spel.",
            Outcome.Draw => "Het spel eindigt gelijk.",
            _ => throw new UnreachableException(),
        };

        _output.WriteLine(result);
    }
}
