namespace RockPaperScissors.Tests.Fakes;

/// <summary>A fake console input that writes every read to a shared event log, so tests can check ordering.</summary>
internal sealed class LoggingReader : TextReader
{
    private readonly Queue<string> _lines;
    private readonly List<string> _log;

    public LoggingReader(List<string> log, params string[] lines)
    {
        _log = log;
        _lines = new Queue<string>(lines);
    }

    public override string? ReadLine()
    {
        var line = _lines.Count > 0 ? _lines.Dequeue() : null;
        _log.Add($"read:{line ?? "<eof>"}");
        return line;
    }
}
