# CLAUDE.md

Rots-Papier-Schaar in C# voor een UWV-sollicitatieopdracht. De beoordeling draait om **codekwaliteit**: structuur, testbaarheid, uitbreidbaarheid en verdedigbare keuzes. Een werkend spel alleen is niet genoeg.

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
dotnet run --project src/RockPaperScissors.Web
```

## Architectuurregels

- `RockPaperScissors.Core` bevat alle spellogica en verwijst naar geen enkel ander project. Geen `Console`, geen ASP.NET/Blazor, geen bestands- of netwerk-I/O.
- Console en Web zijn dunne lagen: invoer lezen, `GameSession` aanroepen, resultaat tonen. Geen spelregels in de interfaces.
- **De history is de enige toestand.** `GameSession` beheert de lijst rondes; al het andere (uitkomst, score, voorspellingen, trefferscores) wordt daaruit berekend. Predictors en `ComputerPlayer` hebben geen velden die tijdens het spel veranderen.
- Geen tellers, tabellen, caches of `Learn`-methodes toevoegen om afgeleide gegevens bij te houden. Lijkt dat nodig (bijv. voor snelheid), stop dan en vraag het eerst.
- **Eerlijkheid is een invariant:** de computer krijgt alleen afgeronde rondes. De zet van de speler in de huidige ronde mag nooit een predictor of `ComputerPlayer` bereiken, ook niet indirect. Bij het naspelen voor trefferscores ziet een predictor alleen de rondes vóór de ronde die hij voorspelt.
- Spelregels staan alleen in de uitkomstmatrix. Geen `if`/`switch` op zetcombinaties elders.
- Nergens uitgaan van precies drie zetten: geen hardcoded 3 of vaste lijst zetten, gebruik `Enum.GetValues<Move>()`.

## Codestijl

- .NET 10 (LTS), nullable reference types aan, warnings as errors.
- File-scoped namespaces, één publiek type per bestand, bestandsnaam = typenaam.
- Klassen `sealed` tenzij overerving bedoeld is. Records voor waardetypen.
- `Random` wordt geïnjecteerd, nooit `new Random()` in een methode of lus, geen statische `Random`.
- Geen magische getallen: exploratiekans en maximum aantal rondes zijn benoemde waarden.
- Kies bij twijfel de eenvoudigste oplossing die de spec volgt. Geen extra abstracties, generics of lagen die de spec niet noemt.
- Code, namen en commentaar in het Engels. Commentaar legt *waarom* uit, niet *wat*.

## Tests

- xUnit, in `tests/RockPaperScissors.Tests`.
- Elke klasse met logica krijgt tests. Testnaam: `Method_Situation_ExpectedResult`.
- Tests zijn deterministisch: geseede `Random` of een nep-implementatie, nooit afhankelijk van toeval of tijd.
- Spelverloop testen via `GameSession` of met een nep-console, niet via echte console-I/O.

## Werkwijze

- Werk in kleine stappen. Een stap is pas klaar als `dotnet build` zonder warnings slaagt en `dotnet test` groen is.
- Geen nieuwe NuGet-packages zonder het eerst te vragen.
- Commit en push alleen als ik daarom vraag. Commitberichten in het Nederlands, kort en beschrijvend.
- Leg na elke stap in het Nederlands kort uit wat je hebt gedaan en waarom. Ik moet elke keuze in een gesprek kunnen verdedigen.
