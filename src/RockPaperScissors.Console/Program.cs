using RockPaperScissors.Console;
using RockPaperScissors.Core;

// Composition root: the only place that creates the Random, once per game, never per round.
new ConsoleGame(
    Console.In,
    Console.Out,
    (totalRounds, opponent) => new GameSession(totalRounds, opponent.Create(new Random()))).Run();
