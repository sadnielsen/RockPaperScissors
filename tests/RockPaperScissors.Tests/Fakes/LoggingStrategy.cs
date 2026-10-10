using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>A strategy that writes every move choice to a shared event log, so tests can check ordering.</summary>
internal sealed class LoggingStrategy : IStrategy
{
    private readonly List<string> _log;

    public LoggingStrategy(List<string> log)
    {
        _log = log;
    }

    public Move ChooseMove(IReadOnlyList<Round> history)
    {
        _log.Add("choose");
        return Move.Rock;
    }
}
