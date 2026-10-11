using RockPaperScissors.Console;
using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;

// Composition root: the only place that picks the concrete predictors and creates the Random.
var computer = new ComputerPlayer(
    [new FrequencyPredictor(), new LastMovePredictor(), new LastMoveAndOutcomePredictor(), new LastTwoMovesPredictor()],
    new Random(),
    ComputerPlayer.DefaultExplorationRate);

new ConsoleGame(Console.In, Console.Out, totalRounds => new GameSession(totalRounds, computer)).Run();
