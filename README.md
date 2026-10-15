# Rots, papier, schaar

Een C#-consoleprogramma waarin een speler rots-papier-schaar speelt tegen de computer. De speler kiest bij de start het aantal rondes (1 tot en met 1000). Elke ronde kiest de computer een willekeurige zet. Na elke ronde toont het programma de uitkomst, aan het eind de totaalscore en de winnaar.

## Starten en testen

Vereist: .NET 10 SDK (vastgelegd in `global.json`).

```
dotnet run --project src/RockPaperScissors.ConsoleUI
dotnet test
```

Zetten voer je in als `r`/`p`/`s` of `rots`/`papier`/`schaar`; hoofdletters en spaties eromheen maken niet uit. Bij ongeldige invoer vraagt het programma opnieuw. Ctrl+Z (Windows) of Ctrl+D (Linux/macOS) sluit netjes af en toont de stand tot dan toe.

## Structuur

```
src/
  RockPaperScissors.Core/      spellogica, geen I/O
  RockPaperScissors.ConsoleUI/   invoer en uitvoer
tests/
  RockPaperScissors.Tests/     xUnit
```

## Hoe de onderdelen samenwerken

**Core**
- `Move` en `Outcome`: enums voor de zetten en de uitkomst (gezien vanuit de speler).
- `Rules.Resolve(player, computer)`: de enige plek die weet wie wint, op basis van een tabel "wint van".
- `Round`: record met de twee zetten. `Outcome` wordt via `Rules` berekend en niet opgeslagen.
- `GameSession`: één spel van n rondes. `PlayRound(move)` laat de computer een willekeurige zet kiezen, voegt de ronde toe en geeft hem terug. `Wins`, `Losses` en `Draws` worden geteld uit de gespeelde rondes, `Result` bepaalt daaruit de winnaar van het spel (pas als het spel klaar is, daarvoor geeft het een exceptie); `IsFinished` wordt waar na n rondes.

**Console**
- `ConsoleGame`: vraagt het aantal rondes, speelt rondes tot `IsFinished` en toont per ronde de uitkomst en aan het eind de score. Werkt op een `TextReader`/`TextWriter` in plaats van op `Console` zelf.
- `MoveText`: vertaalt tussen tekst (`r`, `rots`, ...) en `Move`.
- `Program`: maakt één `Random` en start het spel.

## Bewust eenvoudig gehouden

- **Twee projecten plus tests.** Core bevat de spelregels zonder I/O; de console is een dunne laag daarboven. Zo is de logica los te testen. Meer lagen of projecten voegen bij deze omvang niets toe.
- **Geen interface voor de computerspeler.** Er is maar één gedrag (willekeurig), dus `GameSession` krijgt direct een `Random`. In tests wordt die vervangen door `FakeRandom`, die vooraf bepaalde zetten oplevert. De computerzet hangt alleen van `Random` af en nooit van de zet van de speler, dus de computer kan niet valsspelen.
- **Score afgeleid, niet bijgehouden.** De gespeelde rondes zijn de enige toestand; uitkomst en score worden eruit berekend. Ze kunnen dus nooit uit de pas lopen.
- **Regels in één tabel.** Een tabel "wint van" in plaats van een `if`/`else`-keten of volledige 3x3-matrix: elk paar staat er één keer in, dus de regels kunnen zichzelf niet tegenspreken.
- **Niet voorbereid op meer zetten.** De tabel in `Rules` koppelt elke zet aan de ene zet die hij verslaat. Bij een variant met bijvoorbeeld Vuur en Water verslaat elke zet er twee; dan wordt de tabel `Move → set van zetten` en controleert `Resolve` met `Contains`. Daarnaast krijgt `Move` de nieuwe waarden en `MoveText` de vertalingen. Spelverloop en score hoeven niet te veranderen, want `GameSession` kiest uit `Enum.GetValues<Move>()`. Die aanpassing is klein en blijft op één plek, dus ik heb hem niet vooraf gebouwd.
- **`Random` geïnjecteerd.** Eén instantie per programma, nooit per ronde. Tests zijn daardoor deterministisch.
- **Geen slimme computer.** Een eerdere versie voorspelde het gedrag van de speler (zie de git-historie). Dat valt buiten de opdracht en maakte de code veel groter. Een willekeurige zet is ook speltheoretisch de juiste strategie: niet te verslaan.

## Tests

xUnit, deterministisch, naamgeving `Method_Situation_ExpectedResult`.

- **Rules:** alle 9 combinaties, inclusief gelijkspel.
- **Round:** uitkomst komt overeen met `Rules`.
- **GameSession:** klaar na precies n rondes, score en winnaar kloppen, winnaar opvragen vóór het einde en een ronde na afloop geven een exceptie, ongeldig aantal rondes wordt geweigerd, de computer speelt alle zetten (geseede `Random`).
- **ConsoleGame:** het hele spelverloop via `StringReader`/`StringWriter`: uitkomst per ronde, eindstand en winnaar, ongeldig aantal rondes, ongeldige zetten en einde van de invoer.
- **MoveText:** geldige en ongeldige invoer.
