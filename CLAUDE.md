# CLAUDE.md

Rots-Papier-Schaar in C# voor een UWV-sollicitatieopdracht. Het doel is een kleine, goed gestructureerde en goed testbare oplossing die de auteur volledig kan toelichten. Niet zoveel mogelijk architectuurconcepten.

## Specificatie

DESIGN.md is leidend voor alle ontwerpkeuzes:

@DESIGN.md

- Volg de spec. Twijfel je, of wil je afwijken? Stop en vraag het eerst.
- Wordt een afwijking goedgekeurd, werk DESIGN.md dan in dezelfde stap bij.
- Bouw niets wat in de spec onder "Buiten scope" staat.

## Commando's

```
dotnet build
dotnet test
dotnet run --project src/RockPaperScissors.Console
```

## Architectuurregels

- `RockPaperScissors.Core` bevat alle spellogica en verwijst naar geen ander project. Geen `Console`, geen bestands- of netwerk-I/O.
- De console is een dunne laag: invoer lezen, `GameSession` aanroepen, resultaat tonen. Geen spelregels in de console.
- De gespeelde rondes (`History`) zijn de enige toestand. Uitkomst en score worden daaruit berekend; geen aparte tellers.
- Spelregels staan alleen in `Rules`. Geen `if`/`switch` op zetcombinaties elders.
- De computerzet hangt alleen af van `Random`, nooit van de zet van de speler.
- Geen interfaces, lagen, generics of patronen zonder concrete meerwaarde. Kies bij twijfel de eenvoudigste oplossing.

## Codestijl

- .NET 10 (LTS), nullable reference types aan, warnings as errors.
- File-scoped namespaces, één publiek type per bestand, bestandsnaam = typenaam.
- Klassen `sealed` tenzij overerving bedoeld is. Records voor waardetypen.
- `Random` wordt geïnjecteerd, nooit `new Random()` in een methode of lus, geen statische `Random`.
- Geen magische getallen: het maximum aantal rondes is een benoemde waarde.
- Code, namen en commentaar in het Engels; tekst voor de speler in het Nederlands. Commentaar legt *waarom* uit, niet *wat*.

## Tests

- xUnit, in `tests/RockPaperScissors.Tests`.
- Elke klasse met logica krijgt tests. Testnaam: `Method_Situation_ExpectedResult`.
- Tests zijn deterministisch: `FakeRandom` of een geseede `Random`, nooit afhankelijk van toeval of tijd.
- Spelverloop testen via `GameSession` of `ConsoleGame` met `StringReader`/`StringWriter`, niet via echte console-I/O.

## Werkwijze

- Werk in kleine stappen. Een stap is pas klaar als `dotnet build` zonder warnings slaagt en `dotnet test` groen is.
- Geen nieuwe NuGet-packages zonder het eerst te vragen.
- Commit en push alleen als ik daarom vraag. Commitberichten in het Nederlands, kort en beschrijvend.
- Leg na elke stap in het Nederlands kort uit wat je hebt gedaan en waarom. Ik moet elke keuze in een gesprek kunnen verdedigen.
