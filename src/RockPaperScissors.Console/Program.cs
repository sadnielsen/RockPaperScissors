using System.Text;
using RockPaperScissors.Console;
using RockPaperScissors.Core;

// Composition root: the only place that knows the concrete types. One Random for the whole run,
// shared by every strategy part, instead of new instances per round.
System.Console.OutputEncoding = Encoding.UTF8;

// Windows PowerShell prefixes piped input with a byte order mark, which Console.In passes on as
// part of the first line. A reader that detects byte order marks drops it. Interactive input keeps
// Console.In, so end of input (Ctrl+Z / Ctrl+D) behaves as the terminal expects.
var input = System.Console.IsInputRedirected
    ? new StreamReader(System.Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true)
    : System.Console.In;

var random = new Random();
var rules = new Rules();

var game = new ConsoleGame(
    input,
    System.Console.Out,
    totalRounds => new GameSession(totalRounds, StrategyFactory.CreateDefault(random, rules), rules));

game.Run();
