using RockPaperScissors.Console;

// Composition root: the only place that creates the Random, once per run, never per round.
new ConsoleGame(Console.In, Console.Out, new Random()).Run();
