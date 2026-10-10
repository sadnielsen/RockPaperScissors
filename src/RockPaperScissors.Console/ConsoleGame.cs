using RockPaperScissors.Core;

namespace RockPaperScissors.Console;

/// <summary>
/// Drives a <see cref="GameSession"/> from text input. Reads and writes through
/// <see cref="TextReader"/>/<see cref="TextWriter"/>, so the whole flow is testable without a real console.
/// </summary>
public sealed class ConsoleGame
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Func<int, GameSession> _createSession;

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
        _output.WriteLine("Rots, papier, schaar tegen de computer.");

        if (!TryReadRoundCount(out var totalRounds))
        {
            _output.WriteLine();
            _output.WriteLine("Geen invoer meer. Tot ziens!");
            return;
        }

        var session = _createSession(totalRounds);

        while (!session.IsFinished)
        {
            if (!TryReadMove(session, out var move))
            {
                _output.WriteLine();
                _output.WriteLine($"Spel afgebroken na {session.History.Count} van {session.TotalRounds} rondes.");
                WriteScore(session.Score);
                return;
            }

            WriteRound(session.PlayRound(move));
        }

        _output.WriteLine();
        _output.WriteLine($"Eindstand na {session.TotalRounds} rondes:");
        WriteScore(session.Score);
        _output.WriteLine(VerdictText(session.Score));
    }

    private bool TryReadRoundCount(out int totalRounds)
    {
        while (true)
        {
            _output.Write($"Hoeveel rondes wil je spelen? ({GameSession.MinRounds}-{GameSession.MaxRounds}): ");
            var line = _input.ReadLine();
            if (line is null)
            {
                totalRounds = 0;
                return false;
            }

            if (RoundCountParser.TryParse(line, out totalRounds))
            {
                return true;
            }

            _output.WriteLine(
                $"Ongeldig aantal. Kies een heel getal van {GameSession.MinRounds} tot en met {GameSession.MaxRounds}.");
        }
    }

    private bool TryReadMove(GameSession session, out Move move)
    {
        while (true)
        {
            _output.Write($"Ronde {session.History.Count + 1}/{session.TotalRounds}. Kies (r)ots, (p)apier of (s)chaar: ");
            var line = _input.ReadLine();
            if (line is null)
            {
                move = default;
                return false;
            }

            if (MoveText.TryParse(line, out move))
            {
                return true;
            }

            _output.WriteLine("Ongeldige zet. Typ r, p, s of rots, papier, schaar.");
        }
    }

    private void WriteRound(Round round) =>
        _output.WriteLine(
            $"Jij: {MoveText.NameOf(round.Player)}, computer: {MoveText.NameOf(round.Computer)}. {OutcomeText(round.Outcome)}");

    private void WriteScore(Score score) =>
        _output.WriteLine($"{score.Wins} gewonnen, {score.Losses} verloren, {score.Draws} gelijk.");

    private static string OutcomeText(Outcome outcome) => outcome switch
    {
        Outcome.Win => "Jij wint deze ronde.",
        Outcome.Lose => "De computer wint deze ronde.",
        Outcome.Draw => "Gelijkspel.",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown outcome."),
    };

    private static string VerdictText(Score score) =>
        score.Wins.CompareTo(score.Losses) switch
        {
            > 0 => "Jij hebt gewonnen!",
            < 0 => "De computer heeft gewonnen.",
            _ => "Het is gelijkspel.",
        };
}
