using RockPaperScissors.Core;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>
/// Plays scripted moves (Rock once exhausted) without predictors, and records a copy of every history it receives.
/// </summary>
internal sealed class RecordingComputerPlayer(params Move[] moves) : IComputerPlayer
{
    private readonly Queue<Move> _moves = new(moves);

    public List<IReadOnlyList<Round>> ReceivedHistories { get; } = [];

    public List<IReadOnlyList<Round>> ReceivedInstances { get; } = [];

    public ComputerDecision ChooseMove(IReadOnlyList<Round> history)
    {
        ReceivedInstances.Add(history);
        ReceivedHistories.Add(history.ToList());
        return new ComputerDecision(_moves.TryDequeue(out Move move) ? move : Move.Rock, []);
    }
}
