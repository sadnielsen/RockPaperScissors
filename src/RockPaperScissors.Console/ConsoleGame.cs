using System.Globalization;
using RockPaperScissors.Core;

namespace RockPaperScissors.Console;

/// <summary>
/// The console loop: reads input, drives a <see cref="GameSession"/>, shows the results.
/// Works on a reader and writer instead of the real console, so the whole flow is testable.
/// </summary>
public sealed class ConsoleGame
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Func<int, GameSession> _createSession;

    /// <param name="createSession">Creates the session once the player has chosen the number of rounds.</param>
    public ConsoleGame(TextReader input, TextWriter output, Func<int, GameSession> createSession)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(createSession);

        _input = input;
        _output = output;
        _createSession = createSession;
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

        GameSession session = _createSession(totalRounds.Value);

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
            _output.Write($"Hoeveel rondes wil je spelen (1-{GameSession.MaxRounds})? ");
            string? line = _input.ReadLine();
            if (line is null)
            {
                return null;
            }

            if (int.TryParse(line.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int rounds)
                && rounds >= 1
                && rounds <= GameSession.MaxRounds)
            {
                return rounds;
            }

            _output.WriteLine($"Ongeldig aantal. Kies een heel getal van 1 tot en met {GameSession.MaxRounds}.");
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
            _ => "Gelijkspel.",
        };

        _output.WriteLine($"Jij: {MoveText.NameOf(round.Player)}, computer: {MoveText.NameOf(round.Computer)}. {result}");
    }

    private void WriteScore(GameSession session) =>
        _output.WriteLine(
            $"Stand na {session.History.Count} rondes: gewonnen {session.Wins}, verloren {session.Losses}, gelijk {session.Draws}.");

    private void WriteWinner(GameSession session)
    {
        if (session.Wins > session.Losses)
        {
            _output.WriteLine("Jij wint het spel!");
        }
        else if (session.Losses > session.Wins)
        {
            _output.WriteLine("De computer wint het spel.");
        }
        else
        {
            _output.WriteLine("Het spel eindigt gelijk.");
        }
    }
}
