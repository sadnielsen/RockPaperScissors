using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

namespace RockPaperScissors.Tests.Fakes;

/// <summary>A predictor driven by a function, which records every history it receives.</summary>
internal sealed class FakePredictor(Func<IReadOnlyList<Round>, Move?> predict) : IPredictor
{
    public static FakePredictor Always(Move? move) => new(_ => move);

    public List<IReadOnlyList<Round>> ReceivedHistories { get; } = [];

    public Move? Predict(IReadOnlyList<Round> history)
    {
        ReceivedHistories.Add(history.ToList());
        return predict(history);
    }
}
