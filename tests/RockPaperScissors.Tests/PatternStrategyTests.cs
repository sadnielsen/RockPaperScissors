using RockPaperScissors.Core;
using RockPaperScissors.Core.Prediction;
using RockPaperScissors.Tests.Fakes;

namespace RockPaperScissors.Tests;

public sealed class PatternStrategyTests
{
    private const int Seed = 42;
    private const int Draws = 100;

    private static readonly PatternStrategyOptions NoExploration = new() { ExplorationRate = 0.0 };
    private static readonly PatternStrategyOptions AlwaysExplore = new() { ExplorationRate = 1.0 };

    private readonly Rules _rules = new();

    [Fact]
    public void ChooseMove_AgainstFixedCycle_WinsStructurallyAfterAFewRounds()
    {
        const int totalRounds = 60;
        const int measuredRounds = 30;
        Move[] playerCycle = { Move.Rock, Move.Paper, Move.Scissors };

        var random = new Random(Seed);
        var predictor = new Predictor<LastMoveKey>(new LastMoveContext(), new TransitionTable<LastMoveKey>(random));
        var strategy = new PatternStrategy(predictor, _rules, random, NoExploration);
        var session = new GameSession(totalRounds, strategy, _rules);

        for (var round = 0; round < totalRounds; round++)
        {
            session.PlayRound(playerCycle[round % playerCycle.Length]);
        }

        var computerWins = session.History.TakeLast(measuredRounds).Count(r => r.Outcome == Outcome.Lose);
        Assert.True(computerWins >= measuredRounds * 0.8, $"Computer won only {computerWins} of the last {measuredRounds} rounds.");
    }

    [Fact]
    public void ChooseMove_PredictionWithoutExploration_PlaysCounterOfPrediction()
    {
        var strategy = new PatternStrategy(new FakePredictor(Move.Rock), _rules, new Random(Seed), NoExploration);

        var moves = Enumerable.Range(0, Draws).Select(_ => strategy.ChooseMove(TestHistory.Empty));

        Assert.All(moves, move => Assert.Equal(Move.Paper, move));
    }

    [Fact]
    public void ChooseMove_NoPrediction_PlaysRandom()
    {
        var strategy = new PatternStrategy(new FakePredictor(null), _rules, new Random(Seed), NoExploration);

        var moves = Enumerable.Range(0, Draws).Select(_ => strategy.ChooseMove(TestHistory.Empty)).ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), moves);
    }

    [Fact]
    public void ChooseMove_FullExploration_IgnoresPrediction()
    {
        var strategy = new PatternStrategy(new FakePredictor(Move.Rock), _rules, new Random(Seed), AlwaysExplore);

        var moves = Enumerable.Range(0, Draws).Select(_ => strategy.ChooseMove(TestHistory.Empty)).ToHashSet();

        Assert.Equal(Enum.GetValues<Move>().ToHashSet(), moves);
    }

    [Fact]
    public void ChooseMove_GrowingHistory_LearnsEachRoundOnceWithPrecedingHistory()
    {
        var predictor = new FakePredictor(null);
        var strategy = new PatternStrategy(predictor, _rules, new Random(Seed));
        var history = TestHistory.Of((Move.Rock, Move.Paper), (Move.Paper, Move.Paper), (Move.Scissors, Move.Paper));

        strategy.ChooseMove(history.Take(2).ToArray());
        strategy.ChooseMove(history);

        Assert.Equal(3, predictor.Learned.Count);
        for (var i = 0; i < history.Count; i++)
        {
            Assert.Equal(history.Take(i), predictor.Learned[i].HistoryBefore);
            Assert.Equal(history[i].Player, predictor.Learned[i].Actual);
        }
    }

    [Fact]
    public void ChooseMove_ShrinkingHistory_ThrowsInvalidOperationException()
    {
        var strategy = new PatternStrategy(new FakePredictor(null), _rules, new Random(Seed));
        strategy.ChooseMove(TestHistory.Of((Move.Rock, Move.Paper), (Move.Paper, Move.Paper)));

        Assert.Throws<InvalidOperationException>(() => strategy.ChooseMove(TestHistory.Of((Move.Rock, Move.Paper))));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_ExplorationRateOutsideZeroToOne_ThrowsArgumentOutOfRangeException(double rate)
    {
        var options = new PatternStrategyOptions { ExplorationRate = rate };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PatternStrategy(new FakePredictor(null), _rules, new Random(Seed), options));
    }
}
