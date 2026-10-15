# Rots-Papier-Schaar: ontwerpspecificatie

## Opdracht

Schrijf een programma in C# waarmee een gebruiker Rots-Papier-Schaar tegen de computer speelt. Het spel moet meerdere rondes kunnen spelen voordat het programma afsluit.

- Papier wint van rots, rots wint van schaar, schaar wint van papier.
- De speler kiest bij de start het aantal rondes (1 tot en met 1000).
- De computer kiest per ronde een willekeurige zet.
- Na elke ronde wordt de uitkomst getoond, na afloop de totaalscore en de winnaar.
- Beoordelingsdoel: een kleine, goed gestructureerde en goed testbare oplossing.

## Uitgangspunten

1. **Spellogica los van I/O.** `Core` bevat de regels en het spelverloop; de console leest invoer en toont resultaten.
2. **Regels op één plek.** Alleen `Rules` weet wie wint.
3. **Gespeelde rondes zijn de enige toestand.** Uitkomst en score worden daaruit berekend, niet apart bijgehouden.
4. **Eerlijk.** De computerzet hangt alleen af van `Random`, nooit van de zet van de speler.
5. **Eenvoud.** Geen interfaces, lagen of patronen zonder concrete meerwaarde.

## Domein (Core)

```csharp
public enum Move { Rock, Paper, Scissors }
public enum Outcome { Win, Lose, Draw }   // vanuit de speler gezien

public sealed record Round(Move Player, Move Computer)
{
    public Outcome Outcome => Rules.Resolve(Player, Computer);
}

public static class Rules
{
    public static Outcome Resolve(Move player, Move computer);
}
```

`Rules` gebruikt een tabel "wint van" (`Rock → Scissors`, `Paper → Rock`, `Scissors → Paper`). Gelijke zetten zijn gelijkspel.

## GameSession

```csharp
public sealed class GameSession
{
    public const int MinRounds = 1;
    public const int MaxRounds = 1000;

    public GameSession(int totalRounds, Random random);   // exceptie buiten MinRounds..MaxRounds
    public static bool IsValidTotalRounds(int totalRounds);   // ook gebruikt door de console

    public int TotalRounds { get; }
    public IReadOnlyList<Round> History { get; }
    public bool IsFinished { get; }

    public int Wins { get; }     // geteld uit History
    public int Losses { get; }
    public int Draws { get; }
    public Outcome Result { get; }   // winnaar van het spel vanuit de speler, uit Wins en Losses; exceptie als !IsFinished

    public Round PlayRound(Move playerMove);   // exceptie als IsFinished
}
```

`PlayRound` kiest een willekeurige computerzet met de geïnjecteerde `Random`, voegt de ronde toe en geeft hem terug. De sessie heeft geen eigen lus en doet geen I/O.

## Console

- `ConsoleGame(TextReader, TextWriter, Random)`: vraagt het aantal rondes, maakt een `GameSession`, speelt `while (!session.IsFinished)` met per ronde één zetinvoer en toont na afloop score en winnaar.
- `MoveText`: vertaling tussen tekst en `Move`.
- `Program`: composition root, maakt één `new Random()`.

## Randgevallen

- Ongeldige zet: opnieuw vragen. Geaccepteerd: `r/p/s` en `rots/papier/schaar`, hoofdletterongevoelig, spaties eromheen genegeerd.
- Ongeldig aantal rondes (leeg, geen geheel getal, 0, negatief, meer dan 1000): opnieuw vragen.
- Gelijkspel telt als ronde en wordt apart in de score getoond.
- Einde invoer (`null` van `ReadLine`): netjes afsluiten, met de stand tot dan toe als het spel al bezig was.
- `PlayRound` na de laatste ronde: `InvalidOperationException`.
- `Result` voordat alle rondes gespeeld zijn: `InvalidOperationException`. Een half gespeeld spel heeft geen winnaar; bij afgebroken invoer toont de console alleen de stand.

## Projectstructuur

```
RockPaperScissors.sln
src/
  RockPaperScissors.Core/      Move, Outcome, Round, Rules, GameSession
  RockPaperScissors.ConsoleUI/   ConsoleGame, MoveText, Program
tests/
  RockPaperScissors.Tests/     xUnit
```

## Tests (minimaal)

- `Rules`: alle 9 combinaties.
- `Round`: uitkomst volgt `Rules`.
- `GameSession`: klaar na precies n rondes, score en `Result` kloppen, `Result` geeft een exceptie vóór het einde, weigert een ronde na afloop, weigert een ongeldig aantal rondes.
- `ConsoleGame`: spelverloop, ongeldige invoer, einde invoer, eindstand.
- `MoveText`: geldige en ongeldige invoer.

Tests zijn deterministisch: `FakeRandom` (vooraf bepaalde computerzetten) of een geseede `Random`.

## Buiten scope

Een voorspellende computer, een webinterface, extra zetten en opslag van gespeelde rondes. Die vallen buiten de opdracht.
