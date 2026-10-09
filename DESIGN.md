# Rots-Papier-Schaar: ontwerpspecificatie

Deze specificatie legt de ontwerpkeuzes vast voor de UWV-softwareopdracht. Gebruik dit document als bron voor de implementatie.

## Opdracht

Schrijf een programma in C# dat Rots-Papier-Schaar speelt tussen de computer en een echte speler. Het spel moet n keer gespeeld kunnen worden voordat het programma afsluit.

- Papier wint van rots, rots wint van schaar, schaar wint van papier.
- **Interpretatie "n keer":** de speler kiest bij de start het aantal rondes (n ≥ 1).
- **Beoordelingsdoel:** codekwaliteit (structuur, testbaarheid, uitbreidbaarheid), niet alleen een werkend spel.

## Uitgangspunten

1. **Regels zijn data, geen if-else.** De uitkomsten staan in een uitkomstmatrix. Uitbreiden (bijv. Rots-Papier-Schaar-Hagedis-Spock) betekent alleen de matrix aanpassen.
2. **De computer speelt eerlijk, en dat is afdwingbaar.** Een strategie krijgt alleen de historie van afgelopen rondes. De computer kiest zijn zet *voordat* de invoer van de speler gelezen wordt.
3. **Spellogica staat los van de interface.** De logica is zonder console of browser te testen. Twee interfaces (console en web) op dezelfde `Core` bewijzen die scheiding.
4. **Uitbreiden door toevoegen, niet door wijzigen** (open/closed). Een nieuwe tactiek is een nieuwe klasse.

## Domein

```csharp
public enum Move { Rock, Paper, Scissors }
public enum Outcome { Win, Lose, Draw }   // vanuit de speler gezien

public record Round(Move Player, Move Computer, Outcome Outcome);
```

### Uitkomstmatrix

Eén plek die bepaalt wie wint (`Rules` of `OutcomeMatrix`):

- `Outcome Resolve(Move player, Move computer)`
- `Move CounterOf(Move move)`: de zet die `move` verslaat.

Rij = zet speler, kolom = zet computer:

| | Rock | Paper | Scissors |
|---|---|---|---|
| **Rock** | Draw | Lose | Win |
| **Paper** | Win | Draw | Lose |
| **Scissors** | Lose | Win | Draw |

## Computerstrategie

```csharp
public interface IStrategy
{
    Move ChooseMove(IReadOnlyList<Round> history);
}
```

`history` bevat alleen afgeronde rondes. De strategie heeft geen toegang tot de huidige invoer van de speler.

Leren gebeurt na de ronde: ofwel leidt de strategie alles af uit `history`, ofwel krijgt ze na elke ronde een aparte `Learn`-aanroep. Kies één aanpak en houd die consequent.

### RandomStrategy

Kiest uniform random. Dit is het speltheoretische evenwicht: niet te verslaan, maar wint gemiddeld ook niets. `Random` wordt geïnjecteerd (seedbaar in tests) en niet per ronde opnieuw aangemaakt.

### Patroonherkenning: context, tabel, strategie

Drie losse verantwoordelijkheden:

| Onderdeel | Verantwoordelijkheid |
|---|---|
| Context | **Wat** is relevant? Zet de historie om in een sleutel: de aanname over de speler. Geen voorspelling. |
| `TransitionTable<TKey>` | **Tellen:** per sleutel hoe vaak elke volgende zet voorkwam. |
| Strategie | **Beslissen:** voorspelling ophalen, counter kiezen via de uitkomstmatrix, deels random spelen. |

Kort gezegd: de context is de hypothese, de tabel is het geheugen, de strategie is de beslisser.

#### Context (generiek, met records als sleutel)

```csharp
public interface IContext<TKey> where TKey : struct
{
    // null = nog te weinig historie
    TKey? GetKey(IReadOnlyList<Round> history);
}
```

Sleutels zijn `readonly record struct`'s, zodat ze op inhoud vergeleken worden en direct als `Dictionary`-sleutel werken. Geen string-sleutels.

Te bouwen contexten:

| Context | Sleutel | Rijen | Vangt |
|---|---|---|---|
| `LastMoveContext` | vorige zet speler | 3 | "na rots speelt hij papier", vaste cycli |
| `LastMoveWithOutcomeContext` | vorige zet + uitkomst | 9 | win-stay, lose-shift (gemiddeld het meest voorspellend bij mensen) |
| `Last2MovesContext` | laatste twee zetten | 9 | langere reeksen |

#### TransitionTable

- Opslag: `Dictionary<TKey, int[]>` (één teller per zet).
- **Laplace-smoothing:** elke cel start op 1, zodat er na één ronde geen harde conclusie volgt.
- `Record(TKey key, Move next)`: cel +1.
- `Predict(TKey key)`: zet met de hoogste telling. Bij gelijke stand: random uit de koplopers.

#### Predictor (verbergt het sleuteltype)

Contexten met verschillende sleuteltypes moeten samen in één lijst kunnen. Daarom een niet-generieke laag:

```csharp
public interface IPredictor
{
    Move? Predict(IReadOnlyList<Round> history);         // null = geen voorspelling
    void Learn(IReadOnlyList<Round> historyBefore, Move actual);
}
```

`Predictor<TKey>` koppelt één context aan één eigen tabel. Elke context heeft zijn eigen tabel; sleutels van verschillende contexten worden nooit gemengd.

#### PatternStrategy

1. Vraag de predictor om een voorspelling.
2. Geen voorspelling (bijv. ronde 1), of random trekking onder de exploratiekans (bijv. 20%): speel random.
3. Anders: speel `CounterOf(voorspelling)`.

De exploratiekans is configureerbaar, zodat de computer zelf niet voorspelbaar wordt.

#### EnsembleStrategy

- Houdt meerdere predictors bij, elk met een score.
- Na elke ronde: elke predictor die goed voorspelde krijgt +1. Scores zakken langzaam weg (decay, bijv. × 0,9 per ronde), zodat de computer omschakelt als de speler van tactiek wisselt.
- Volgt de predictor met de hoogste score. Dat lost ook *data sparsity* op: in het begin wint de eenvoudige context, later eventueel een complexere.

## Spelverloop: GameSession

`GameSession` staat in `Core` en bevat alle spellogica rond rondes. Console en web gebruiken dezelfde sessie; ze verschillen alleen in **wie de rondes aanstuurt**.

```csharp
public class GameSession
{
    public GameSession(int totalRounds, IStrategy strategy, Rules rules) { ... }

    public int TotalRounds { get; }
    public IReadOnlyList<Round> History { get; }
    public bool IsFinished => History.Count >= TotalRounds;
    // plus score: gewonnen / verloren / gelijk

    public Round PlayRound(Move playerMove);   // gooit exceptie als IsFinished
}
```

Binnen `PlayRound`, in deze volgorde:

1. Computer kiest zijn zet: de strategie krijgt alleen `History`, nooit `playerMove`.
2. Uitkomst bepalen via de uitkomstmatrix.
3. Ronde toevoegen aan de historie en de strategie laten leren.

De sessie heeft geen eigen lus en doet geen I/O.

- **Console:** vraagt het aantal rondes, maakt een sessie, draait `while (!session.IsFinished)` met per ronde één invoer, toont na afloop de eindstand.
- **Web:** maakt een sessie bij "Start spel" en bewaart die (per gebruiker/circuit). Elke klik op Rots/Papier/Schaar is één `PlayRound`.

## Randgevallen

- Ongeldige zetinvoer: opnieuw vragen, niet crashen. Accepteer bijv. `r/p/s` of `rots/papier/schaar`, hoofdletterongevoelig.
- Ongeldig aantal rondes (leeg, geen getal, 0, negatief, absurd groot): opnieuw vragen.
- Gelijkspel telt als ronde en wordt apart bijgehouden in de score.
- Einde invoer (Ctrl+Z / Ctrl+D, `null` van `ReadLine`): netjes afsluiten.
- `PlayRound` aanroepen na de laatste ronde: duidelijke exceptie in `Core`; de interfaces voorkomen dit (web: knoppen uitschakelen).

## Bekende denkfouten om te vermijden

- Winlogica als lange if-else-keten in plaats van de uitkomstmatrix.
- `new Random()` in een lus of per ronde.
- De computer laten kiezen nadat de speler zijn invoer gaf (valsspelen, ook al lijkt het onschuldig).
- Console-I/O vermengd met spellogica, waardoor niets te testen is.
- String-sleutels voor contexten (geen typecontrole).

## Projectstructuur

```
RockPaperScissors.sln
src/
  RockPaperScissors.Core/      domein, regels, strategieën, contexten (geen Console)
  RockPaperScissors.Console/   console-interface, composition root
  RockPaperScissors.Web/       minimale webinterface (Blazor Server of één pagina + minimal API)
tests/
  RockPaperScissors.Tests/     xUnit
README.md                      keuzes en uitleg
```

- .NET 8 (LTS), nullable reference types aan, warnings as errors.
- Bewust geen volledige Clean Architecture met aparte Application/Infrastructure-lagen: voor deze omvang laten Core + twee interfaces hetzelfde principe zien zonder overdaad. Afhankelijkheden lopen alleen naar `Core`.
- Web: minimaal qua vormgeving, het gaat om de scheiding. Bouwvolgorde: eerst console werkend, daarna web.
- Console-I/O achter een kleine abstractie (bijv. `IConsole` of `TextReader`/`TextWriter`), zodat het spelverloop testbaar is.

## Tests (minimaal)

- Uitkomstmatrix: alle 9 combinaties.
- Elke context: juiste sleutel, en `null` bij te weinig historie.
- `TransitionTable`: telt goed, voorspelt de hoogste, start neutraal.
- `PatternStrategy` tegen een vaste cyclus: wint na enkele rondes structureel.
- `EnsembleStrategy`: volgt de predictor die het vaakst goed zit.
- **Eerlijkheid:** de strategie kiest zonder de huidige zet te kennen (bijv. strategie-aanroep vindt plaats vóór het lezen van invoer, aantoonbaar met een nep-console).
- Invoervalidatie: ongeldige invoer leidt tot opnieuw vragen.
- `GameSession`: is klaar na precies n rondes, houdt de score bij, weigert een ronde na afloop.

## Historie-opslag: alleen het haakje

```csharp
public interface IHistoryStore
{
    IReadOnlyList<Round> Load(string playerId);
    void Save(string playerId, IReadOnlyList<Round> history);
}
```

Alleen een `InMemoryHistoryStore` bouwen. Persistente opslag (bijv. JSON-bestand of SQLite) niet bouwen, wel in de README uitleggen:
- Voordeel: meer historie per speler, dus eerder goede voorspellingen (minder data sparsity).
- Nadeel: spelers gedragen zich per sessie anders, dus oude historie voorspelt minder dan verwacht.
- Kosten: gebruikers identificeren, opslag kiezen, en AVG (gedragsgegevens van personen opslaan).

## Buiten scope (wel noemen in README)

De README krijgt een sectie "Wat ik bewust niet heb gedaan, en waarom" met de punten hieronder en de persistente opslag.


- **AI-strategie** (Claude/OpenAI met structured output, of JEV): past achter `IStrategy`, alleen historie meesturen, terugvallen op random bij fouten, API-sleutel uit configuratie. Niet standaard, want de beoordelaar moet het spel zonder sleutel of internet kunnen draaien.
- Backoff naar eenvoudigere context bij een lege rij.

## Termen

Markov-keten / transitiematrix, n-gram-model, Laplace-smoothing, opponent modelling, ensemble / prediction with expert advice, exponential decay. Inspiratie: RPS-programmeerwedstrijden ("Iocaine Powder", Dan Egnor, 1999).
