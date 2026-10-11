namespace RockPaperScissors.Tests.Fakes;

/// <summary>A <see cref="Random"/> that returns scripted values, so tests can steer every random choice.</summary>
internal sealed class FakeRandom : Random
{
    private readonly Queue<int> _ints;

    /// <param name="nextDouble">Returned by every <see cref="NextDouble"/> call; the default never triggers exploration.</param>
    /// <param name="ints">Returned in order by <see cref="Next(int)"/>; 0 once exhausted.</param>
    public FakeRandom(double nextDouble = 0.99, params int[] ints)
    {
        NextDoubleValue = nextDouble;
        _ints = new Queue<int>(ints);
    }

    public double NextDoubleValue { get; }

    public List<int> RequestedMaxValues { get; } = [];

    public override double NextDouble() => NextDoubleValue;

    public override int Next(int maxValue)
    {
        RequestedMaxValues.Add(maxValue);
        return _ints.TryDequeue(out int value) ? value : 0;
    }
}
