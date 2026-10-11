# Rots-Papier-Schaar: ontwerpspecificatie

Deze specificatie legt de ontwerpkeuzes vast voor de UWV-softwareopdracht. Gebruik dit document als bron voor de implementatie.

## Opdracht

Schrijf een programma in C# dat Rots-Papier-Schaar speelt tussen de computer en een echte speler. Het spel moet n keer gespeeld kunnen worden voordat het programma afsluit.

- Papier wint van rots, rots wint van schaar, schaar wint van papier.
- **Interpretatie "n keer":** de speler kiest bij de start het aantal rondes (n ≥ 1).
- **Beoordelingsdoel:** codekwaliteit (structuur, testbaarheid, uitbreidbaarheid), niet alleen een werkend spel.

## Kernidee: alles afleiden uit de history

De **history** (de lijst gespeelde rondes) is de enige toestand in het spel. Al het andere wordt daar op het moment zelf uit berekend:

| Gegeven | Afgeleid uit de history |
|---|---|
| Uitkomst van een ronde | regels toepassen op de twee zetten |
| Score | uitkomsten tellen |
| Voorspelling van een predictor | patroon zoeken in de history |
| Welke predictor het beste is | tellen hoe vaak de vastgelegde voorspelling van elke predictor klopte |

Elke ronde legt naast de twee zetten ook vast wat elke predictor vóór die ronde voorspelde. Dat is geen afgeleide toestand naast de history, maar een feit van dat moment ("wat dacht de computer vóór deze zet"), net als de zet van de computer zelf: eenmalig vastgelegd, onveranderlijk, en ín de history.

Er zijn geen tellers, tabellen, scores of `Learn`-aanroepen die bijgewerkt moeten worden. Predictors en de computerspeler zijn **stateless**: elke beslissing is een pure functie van wat er gespeeld is (plus het random-getal voor exploratie).

**Waarom:**
- Eén bron van waarheid: afgeleide gegevens kunnen nooit uit de pas lopen met de history.
- Testen is eenvoudig: geef een vaste history, controleer de uitkomst. Geen opbouw van interne toestand nodig.
- Minder code en minder begrippen dan een opzet met lerende tabellen.
- Opslaan of herladen van een spel is triviaal: wie de history heeft, heeft alles.

**Prijs:** de computer rekent elke ronde opnieuw: elke predictor voorspelt één keer op de hele history, en de trefferscores zijn een telling. Dat is O(P·r) per ronde, verwaarloosbaar bij het maximale aantal rondes. Eerder werd de trefferscore berekend door de history na te spelen (O(P·r²) per ronde); waarom dat is veranderd staat in de README.

## Uitgangspunten

1. **History is de enige toestand.** Niets naast de history opslaan wat eruit af te leiden is. Geen caches. Wat de computer op een moment besloot of verwachtte, hoort als feit bij de ronde.
2. **Regels staan op één plek.** De uitkomstmatrix bepaalt wie wint; nergens anders `if`/`switch` op zetcombinaties.
3. **De computer speelt eerlijk, en dat is afdwingbaar.** De computer krijgt alleen afgeronde rondes en kiest zijn zet voordat de zet van de speler verwerkt wordt.
4. **Spellogica staat los van de interface.** Twee interfaces (console en web) op dezelfde `Core` bewijzen die scheiding.
5. **Uitbreiden door toevoegen** (open/closed). Een nieuwe tactiek is een nieuwe predictor-klasse.
6. **Voorbereid op meer zetten, zonder ze te bouwen.** Het spel heeft drie zetten, maar niets in de code gaat uit van precies drie (zie "Meer zetten").

## Domein

```csharp
public enum Move { Rock, Paper, Scissors }
public enum Outcome { Win, Lose, Draw }   // vanuit de speler gezien

// Predictions: wat elke predictor van de computer vóór deze ronde voorspelde, in de volgorde van zijn predictors.
public sealed record Round(Move Player, Move Computer, IReadOnlyList<Move?> Predictions)
{
    public Round(Move player, Move computer) : this(player, computer, []) { }   // computer zonder predictors

    public Outcome Outcome => Rules.Resolve(Player, Computer);   // afgeleid, niet opgeslagen
}
```

`Round` heeft waarde-gelijkheid, ook over `Predictions` (een record vergelijkt een lijst anders op referentie).

### Rules (uitkomstmatrix)

Statische klasse, de enige plek die weet wie wint:

- `Outcome Resolve(Move player, Move computer)`
- `IReadOnlyList<Move> CountersOf(Move move)`: de zetten die `move` verslaan. Bij drie zetten is dat er altijd één; bij vijf zijn het er twee.

Rij = zet speler, kolom = zet computer:

| | Rock | Paper | Scissors |
|---|---|---|---|
| **Rock** | Draw | Lose | Win |
| **Paper** | Win | Draw | Lose |
| **Scissors** | Lose | Win | Draw |

### Meer zetten

Rots-Papier-Schaar is een **intransitief** spel (cyclische dominantie): elke zet wint van precies de helft van de andere. Dat werkt met elk oneven aantal zetten, bijv. vijf met Vuur en Water:

| Wint van → | Rock | Paper | Scissors | Fire | Water |
|---|---|---|---|---|---|
| **Rock** | | | ✓ | ✓ | |
| **Paper** | ✓ | | | | ✓ |
| **Scissors** | | ✓ | | | ✓ |
| **Fire** | | ✓ | ✓ | | |
| **Water** | ✓ | | | ✓ | |

**Niet bouwen**, wel zo ontwerpen dat uitbreiden alleen deze plekken raakt:
1. `Move` krijgt nieuwe waarden.
2. De uitkomstmatrix in `Rules` wordt groter.
3. De vertaling van invoer en weergave in Console en Web (bijv. `v` = Vuur).

Daarvoor geldt:
- Nergens het getal 3 of een vaste lijst zetten hardcoden; gebruik `Enum.GetValues<Move>()`.
- Predictors en random keuzes werken over alle waarden van `Move`.
- `CountersOf` geeft een lijst, omdat er bij meer zetten meerdere counters zijn.

In de README noemen als bewijs van uitbreidbaarheid, met de 5x5-tabel als voorbeeld.

## Predictors

Een predictor voorspelt de volgende zet van de speler. Hij kiest geen zet en onthoudt niets.

```csharp
public interface IPredictor
{
    // null = geen voorspelling (te weinig history, of geen duidelijke koploper)
    Move? Predict(IReadOnlyList<Round> history);
}
```

### Gedeelde helper: "wat volgde er eerder op deze situatie?"

Bijna elke predictor stelt dezelfde vraag: *zoek de eerdere momenten die lijken op nu, en tel wat de speler daarna speelde.* Dat zit in één interne helper:

```csharp
internal static class PatternCounter
{
    // Bekijkt elke eerdere ronde i (i ≥ 1). Als de situatie vóór ronde i
    // overeenkomt met de huidige (matchesSituationBefore(i) is true),
    // telt de zet die de speler in ronde i speelde.
    // Geeft de zet met de hoogste telling, of null bij geen treffers of een gelijke stand.
    public static Move? MostFrequentNext(
        IReadOnlyList<Round> history,
        Func<int, bool> matchesSituationBefore);
}
```

Een predictor is daardoor alleen nog de beschrijving van "lijkt op nu":

```csharp
public sealed class LastMovePredictor : IPredictor
{
    public Move? Predict(IReadOnlyList<Round> history)
    {
        if (history.Count == 0) return null;
        Move last = history[^1].Player;
        return PatternCounter.MostFrequentNext(history, i => history[i - 1].Player == last);
    }
}
```

### Te bouwen predictors

| Predictor | "Lijkt op nu" als... | Vangt |
|---|---|---|
| `FrequencyPredictor` | (altijd: telt alle zetten) | voorkeur voor één zet, bijv. vaak rots |
| `LastMovePredictor` | de vorige zet van de speler gelijk is | "na rots speelt hij papier", vaste cycli |
| `LastMoveAndOutcomePredictor` | vorige zet én uitkomst gelijk zijn | win-stay, lose-shift (gemiddeld het meest voorspellend bij mensen) |
| `LastTwoMovesPredictor` | de laatste twee zetten van de speler gelijk zijn | langere reeksen |

`FrequencyPredictor` gebruikt de helper niet: hij telt gewoon alle zetten van de speler.

## ComputerPlayer: de enige die een zet kiest

```csharp
public sealed record ComputerDecision(Move Move, IReadOnlyList<Move?> Predictions);

public interface IComputerPlayer
{
    ComputerDecision ChooseMove(IReadOnlyList<Round> history);
}

public sealed class ComputerPlayer : IComputerPlayer
{
    public ComputerPlayer(IEnumerable<IPredictor> predictors, Random random, double explorationRate);
}
```

`ChooseMove(history)`:

1. **Voorspellen:** elke predictor voorspelt één keer op de history. Altijd, ook als de computer gaat exploreren: elke ronde moet elke voorspelling vastleggen, anders missen latere trefferscores rondes.
2. **Exploratie:** met kans `explorationRate` (standaard 0,2) een random zet. Zo wordt de computer zelf niet voorspelbaar.
3. **Beste predictor kiezen:** voor elke predictor de **trefferscore** berekenen (zie hieronder) en de hoogste nemen. Bij gelijke score wint de predictor die eerder in de lijst staat.
4. **Counteren:** geeft de beste predictor een voorspelling, speel dan een zet uit `Rules.CountersOf(voorspelling)` (bij meer dan één: random daartussen).
5. **Geen voorspelling:** random zet.
6. Geef de zet en alle voorspellingen terug als `ComputerDecision`.

### Trefferscore (geteld uit de vastgelegde voorspellingen)

```
Score(predictor p, history) =
    aantal rondes waarvoor round.Predictions[p] == round.Player
```

Oftewel: "hoe vaak had deze predictor gelijk sinds het begin?" Elke voorspelling is gemaakt voordat de zet van de speler in die ronde bekend was, dus de score is net zo eerlijk als het spel zelf.

Voorspellingen worden op positie aan predictors gekoppeld. Heeft een ronde een ander aantal voorspellingen dan de computer predictors heeft, dan geeft `ChooseMove` een `ArgumentException`: liever luid falen dan stil verkeerde scores.

Gevolg: in het begin scoort een eenvoudige predictor het best (die heeft het snelst genoeg data), later neemt een specifiekere het over als die vaker gelijk heeft. Dat lost het *data sparsity*-probleem zonder extra code op.

### Random spelen

Een `ComputerPlayer` zonder predictors speelt altijd random. Er is dus geen aparte `RandomStrategy` nodig. `Random` wordt geïnjecteerd (seedbaar in tests) en nooit per ronde opnieuw aangemaakt.

## Spelverloop: GameSession

`GameSession` staat in `Core`, beheert de history en is de enige plek waar die groeit. Console en web gebruiken dezelfde sessie; ze verschillen alleen in **wie de rondes aanstuurt**.

```csharp
public sealed class GameSession
{
    public GameSession(int totalRounds, IComputerPlayer computer);

    public int TotalRounds { get; }
    public IReadOnlyList<Round> History { get; }
    public bool IsFinished => History.Count >= TotalRounds;

    // afgeleid uit History
    public int Wins { get; }
    public int Losses { get; }
    public int Draws { get; }

    public Round PlayRound(Move playerMove);   // exceptie als IsFinished
}
```

Binnen `PlayRound`, in deze volgorde:

1. `computer.ChooseMove(History)`: de computer krijgt alleen afgeronde rondes, nooit `playerMove`. Zijn zet en de voorspellingen liggen dus vast voordat `playerMove` gebruikt wordt.
2. `new Round(playerMove, decision.Move, decision.Predictions)` toevoegen aan de history.
3. De ronde teruggeven (de uitkomst zit erin als afgeleide property).

De sessie heeft geen eigen lus en doet geen I/O.

- **Console:** vraagt het aantal rondes, maakt een sessie, draait `while (!session.IsFinished)` met per ronde één invoer, toont na afloop de eindstand.
- **Web:** maakt een sessie bij "Start spel" en bewaart die (per gebruiker/circuit). Elke klik op Rots/Papier/Schaar is één `PlayRound`.

## Randgevallen

- Ongeldige zetinvoer: opnieuw vragen, niet crashen. Accepteer bijv. `r/p/s` of `rots/papier/schaar`, hoofdletterongevoelig.
- Ongeldig aantal rondes (leeg, geen getal, 0, negatief, meer dan 1000): opnieuw vragen. Het maximum houdt ook het opnieuw rekenen per ronde beperkt.
- Gelijkspel telt als ronde en wordt apart getoond in de score.
- Einde invoer (Ctrl+Z / Ctrl+D, `null` van `ReadLine`): netjes afsluiten.
- `PlayRound` aanroepen na de laatste ronde: duidelijke exceptie in `Core`; de interfaces voorkomen dit (web: knoppen uitschakelen).
- Predictor met lege history of zonder duidelijke koploper: `null`, nooit een exceptie.

## Bekende denkfouten om te vermijden

- Winlogica als lange if-else-keten in plaats van de uitkomstmatrix.
- Afgeleide gegevens opslaan (uitkomst, score, tellers) naast de history, waardoor ze uit de pas kunnen lopen.
- Een cache van voorspellingen of scores in `ComputerPlayer`: die maakt hem stateful en gaat mis zodra dezelfde computer een nieuw spel speelt (in de webapp wordt hij per circuit hergebruikt).
- `new Random()` in een lus of per ronde.
- De computer laten kiezen nadat de speler zijn invoer gaf (valsspelen, ook al lijkt het onschuldig).
- Een voorspelling vastleggen of bijwerken nadat de zet van de speler bekend is.
- Console-I/O vermengd met spellogica, waardoor niets te testen is.

## Projectstructuur

```
RockPaperScissors.sln
src/
  RockPaperScissors.Core/      domein, Rules, predictors, ComputerPlayer, GameSession (geen I/O)
  RockPaperScissors.Console/   console-interface, composition root
  RockPaperScissors.Web/       minimale webinterface (Blazor Server)
tests/
  RockPaperScissors.Tests/     xUnit
README.md                      keuzes en uitleg
```

- .NET 10 (LTS), nullable reference types aan, warnings as errors.
- Bewust geen volledige Clean Architecture met aparte Application/Infrastructure-lagen: voor deze omvang laten Core + twee interfaces hetzelfde principe zien zonder overdaad. Afhankelijkheden lopen alleen naar `Core`.
- Web: minimaal qua vormgeving, het gaat om de scheiding.
- Console-I/O achter een kleine abstractie (bijv. `TextReader`/`TextWriter`), zodat het spelverloop testbaar is.
- Composition root (Console en Web): `new ComputerPlayer([new FrequencyPredictor(), new LastMovePredictor(), new LastMoveAndOutcomePredictor(), new LastTwoMovesPredictor()], new Random(), 0.2)`.

## Tests (minimaal)

- `Rules`: alle 9 combinaties en `CountersOf` voor elke zet.
- `Rules` is gebalanceerd: elke zet wint van precies evenveel zetten als hij van verliest. Deze test blijft geldig bij uitbreiding en bewaakt een foute matrix.
- Elke predictor: juiste voorspelling op een handgemaakte history, `null` bij lege history en bij een gelijke stand.
- `PatternCounter`: telt alleen rondes waarvan de situatie ervoor overeenkomt.
- `ComputerPlayer`:
  - volgt de predictor met de meeste vastgelegde treffers (met nep-predictors);
  - speelt de counter van de voorspelling;
  - speelt random zonder predictors of zonder voorspelling;
  - exploratie met een geseede `Random`, en ook dan worden voorspellingen vastgelegd;
  - weigert een history met een ander aantal voorspellingen per ronde.
- **Simulatie** (geseede `Random`, 1000 rondes): computer tegen een vaste cyclus wint ruim; tegen win-stay/lose-shift wint duidelijk; tegen een random speler blijft winst, verlies en gelijkspel elk rond een derde.
- **Eerlijkheid:** de computer krijgt de zet van de huidige ronde niet te zien (bijv. een nep-`IComputerPlayer` die de ontvangen history vastlegt), en de vastgelegde voorspelling in ronde k is gemaakt op precies de k−1 rondes ervoor.
- `GameSession`: klaar na precies n rondes, score klopt met de history, weigert een ronde na afloop.
- Invoervalidatie: ongeldige invoer leidt tot opnieuw vragen.

## Historie-opslag: alleen het haakje

Omdat de history alles is, is opslag eenvoudig: sla de rondes op en alles is terug te rekenen.

```csharp
public interface IHistoryStore
{
    IReadOnlyList<Round> Load(string playerId);
    void Save(string playerId, IReadOnlyList<Round> history);
}
```

Alleen een `InMemoryHistoryStore` bouwen. Persistente opslag (bijv. JSON-bestand of SQLite) niet bouwen, wel in de README uitleggen:
- Voordeel: meer history per speler, dus eerder goede voorspellingen.
- Nadeel: spelers gedragen zich per sessie anders, dus oude history voorspelt minder dan verwacht.
- Nadeel: opgeslagen rondes bevatten voorspellingen van de predictors van dat moment. Verandert de set predictors, dan past een oude history niet meer (`ChooseMove` weigert hem). Je moet de history dan migreren, of de voorspellingen per predictor-naam vastleggen in plaats van op positie.
- Kosten: gebruikers identificeren, opslag kiezen, en AVG (gedragsgegevens van personen opslaan).

## Buiten scope (wel noemen in README)

De README krijgt een sectie "Wat ik bewust niet heb gedaan, en waarom" met deze punten en de persistente opslag:

- **AI-predictor** (Claude/OpenAI met structured output, of JEV): past achter `IPredictor`, alleen history meesturen, `null` bij fouten. Niet standaard, want de beoordelaar moet het spel zonder sleutel of internet kunnen draaien.
  - Vraagt om **async**: `ValueTask<Move?> PredictAsync(history, CancellationToken)`, wat doorwerkt naar `ComputerPlayer`, `GameSession`, de console-lus en de tests. Een mechanische aanpassing, maar door alle lagen.
  - De **vastgelegde voorspellingen** zijn er al: een externe predictor wordt per ronde één keer aangeroepen, niet één keer per eerdere ronde.
- **Recente rondes zwaarder laten wegen** in de trefferscore (decay), zodat de computer sneller omschakelt als de speler van tactiek wisselt. Kan als kleine aanpassing in de telling.

## Termen

- Single source of truth, pure functies, stateless.
- Vergelijkbaar met het idee achter *event sourcing*: de lijst gebeurtenissen is de waarheid, toestand wordt eruit afgeleid.
- Markov-model / n-gram-model (wat de predictors in feite doen), opponent modelling.
- Ensemble / *prediction with expert advice* (het kiezen van de beste predictor op basis van eerdere treffers).
- Inspiratie: RPS-programmeerwedstrijden ("Iocaine Powder", Dan Egnor, 1999).
