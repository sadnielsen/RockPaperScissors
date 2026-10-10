using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>Returns a fixed prediction and records every Learn call.</summary>
internal sealed class FakePredictor : IPredictor
{
    private readonly List<(Round[] HistoryBefore, Move Actual)> _learned = new();

    public FakePredictor(Move? prediction)
    {
        Prediction = prediction;
    }

    public Move? Prediction { get; set; }

    public IReadOnlyList<(Round[] HistoryBefore, Move Actual)> Learned => _learned;

    public Move? Predict(IReadOnlyList<Round> history) => Prediction;

    public void Learn(IReadOnlyList<Round> historyBefore, Move actual) =>
        _learned.Add((historyBefore.ToArray(), actual));
}
