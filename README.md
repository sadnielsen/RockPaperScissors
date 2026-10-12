# Rots, papier, schaar

Een C#-programma dat rots-papier-schaar speelt tussen de computer en een echte speler. De speler kiest bij de start het aantal rondes (1 tot en met 1000) en een tegenstander. Er zijn twee interfaces op dezelfde spellogica: een console-app en een minimale Blazor Server-webapp.

De tegenstanders zijn een soort moeilijkheidsniveaus. De makkelijkste gokt. De andere zoeken elk één soort patroon in wat de speler eerder deed en spelen de zet die daartegen wint. Tegen een speler die in dat patroon valt, wint zo'n tegenstander ongeveer 87% van de rondes; tegen een willekeurige speler blijft het rond een derde.

## Draaien en testen

Vereist: .NET 10 SDK (vastgelegd in `global.json`).

```
dotnet build
dotnet test
dotnet run --project src/RockPaperScissors.Console
dotnet run --project src/RockPaperScissors.Web
```

- **Console:** kies eerst het aantal rondes en dan een tegenstander (nummer, of Enter voor de standaard). Zetten voer je in als `r`/`p`/`s` of `rots`/`papier`/`schaar`, hoofdletters maken niet uit. Ongeldige invoer leidt tot een nieuwe vraag. Ctrl+Z (Windows) of Ctrl+D (Linux/macOS) sluit netjes af.
- **Web:** open de URL die in de console verschijnt (standaard `http://localhost:5256`).
- **Tests:** de volledige suite, inclusief zeven simulaties van 1000 rondes, draait in minder dan een halve seconde.

## Structuur

```
src/
  RockPaperScissors.Core/      spellogica: domein, Rules, predictors, computerspelers, Opponents, GameSession (geen I/O)
  RockPaperScissors.Console/   console-interface en composition root
  RockPaperScissors.Web/       Blazor Server-interface en composition root
tests/
  RockPaperScissors.Tests/     xUnit
```

Afhankelijkheden lopen alleen naar `Core`; `Core` kent niets anders. Console en Web lezen invoer, roepen `GameSession` aan en tonen het resultaat. Ze bevatten geen spelregels. Dat er twee interfaces zijn, is het bewijs dat die scheiding werkt.

Ik heb bewust geen volledige Clean Architecture met aparte Application- en Infrastructure-lagen gebouwd. Voor deze omvang laten Core plus twee interfaces hetzelfde principe zien zonder overdaad.

## Kernidee: de history is de enige toestand

De lijst gespeelde rondes is het enige wat het spel bijhoudt. Een ronde is alleen de twee zetten. Al het andere wordt er op het moment zelf uit berekend:

| Gegeven | Afgeleid uit de history |
|---|---|
| Uitkomst van een ronde | `Round.Outcome` past de regels toe op de twee zetten |
| Score | uitkomsten tellen |
| Voorspelling van een predictor | patroon zoeken in de history |

Er zijn geen tellers, tabellen, caches of `Learn`-methodes. Predictors en computerspelers hebben geen velden die tijdens het spel veranderen; elke beslissing is een pure functie van de history (plus een random getal).

Waarom:
- **Eén bron van waarheid.** Afgeleide gegevens kunnen nooit uit de pas lopen met de history.
- **Eenvoudig testen.** Geef een vaste history, controleer de uitkomst. Je hoeft geen interne toestand op te bouwen.
- **Minder code en minder begrippen** dan een opzet met lerende tabellen.
- **Opslaan is triviaal:** wie de history heeft, heeft alles.

Het idee lijkt op *event sourcing*: de lijst gebeurtenissen is de waarheid, toestand wordt eruit afgeleid.

De prijs: de computer loopt elke ronde één keer door de history. Bij maximaal 1000 rondes is dat verwaarloosbaar.

## Spelregels: één tabel

`Rules` is de enige plek die weet wie wint. De regels staan in een tabel "wint van" (`Rock → {Scissors}`, ...). `Resolve` en `CountersOf` leiden alles daaruit af. Nergens anders staat een `if` of `switch` op zetcombinaties.

Ik koos een "wint van"-tabel in plaats van een volledige 3x3-uitkomstmatrix omdat elk paar er dan maar één keer in staat. Een volledige matrix kan zichzelf tegenspreken ("rots verliest van papier" én "papier verliest van rots"); deze tabel niet. Een test bewaakt bovendien dat de regels gebalanceerd zijn: elke zet wint van precies evenveel zetten als hij van verliest.

## Eerlijkheid

De computer kiest zijn zet voordat de zet van de speler verwerkt wordt, en krijgt alleen afgeronde rondes te zien. Dat is niet alleen een afspraak, het is afgedwongen en getest:

- `GameSession.PlayRound` roept eerst `computer.ChooseMove(History)` aan en voegt pas daarna de ronde toe. De zet van de speler kan de computer niet bereiken.
- De history gaat naar buiten als read-only weergave.
- Een test met een nep-computerspeler legt vast wat die ontvangt, en controleert dat het in ronde k precies de k−1 afgeronde rondes zijn.

## Hoe de computer kiest

### Eén tegenstander per spel

```
IComputerPlayer
 ├─ RandomComputerPlayer(Random)
 └─ PredictingComputerPlayer(IPredictor, Random, explorationRate)
```

Elk spel heeft precies één computerspeler met één vaste tactiek. De speler kiest hem uit `Opponents.All`, een lijst in Core, zodat Console en Web dezelfde keuze bieden:

| Tegenstander | Computerspeler | Indruk voor de speler |
|---|---|---|
| Random | `RandomComputerPlayer` | makkelijk, geen patroon |
| Frequentie | `PredictingComputerPlayer` + `FrequencyPredictor` | straft een voorkeurszet af |
| Vorige zet | `PredictingComputerPlayer` + `LastMovePredictor` | straft vaste reeksen af |
| Laatste twee zetten | `PredictingComputerPlayer` + `LastTwoMovesPredictor` | straft langere reeksen af |
| Zet + uitkomst (standaard) | `PredictingComputerPlayer` + `LastMoveAndOutcomePredictor` | lastig: straft win-stay/lose-shift af |

Een nieuwe tegenstander is een nieuwe regel in die lijst; een nieuwe tactiek is een nieuwe predictor-klasse. Elke optie maakt per spel een nieuwe computerspeler met een eigen `Random`. Er wordt dus niets gedeeld tussen spellen of gebruikers, wat in de webapp telt: `Random` is niet thread-safe.

De namen in `Opponents` zijn Nederlands, terwijl de rest van Core Engels is. Dat is een bewuste afweging: zo staat de lijst met wat de speler ziet op één plek, in plaats van dat Console en Web elk een vertaling bijhouden.

### Predictors

Een predictor voorspelt de volgende zet van de speler. Hij kiest geen zet en onthoudt niets (`IPredictor.Predict(history)` geeft `Move?`). Bijna elke predictor stelt dezelfde vraag: *wat speelde de speler eerder na een situatie zoals nu?* Die vraag zit in één helper, `PatternCounter`. Een predictor is daardoor alleen nog de beschrijving van "lijkt op nu":

| Predictor | "Lijkt op nu" als... | Vangt |
|---|---|---|
| `FrequencyPredictor` | (telt alle zetten) | voorkeur voor één zet |
| `LastMovePredictor` | de vorige zet gelijk is | "na rots komt papier", vaste cycli |
| `LastMoveAndOutcomePredictor` | vorige zet én uitkomst gelijk zijn | win-stay, lose-shift |
| `LastTwoMovesPredictor` | de laatste twee zetten gelijk zijn | langere reeksen |

Bij te weinig data of een gelijke stand geeft een predictor `null`: liever geen voorspelling dan een gok.

In feite zijn dit kleine Markov- of n-gram-modellen van de tegenstander (*opponent modelling*).

### PredictingComputerPlayer: voorspellen, counteren, soms gokken

De logica die elke tactiek deelt, staat één keer in `PredictingComputerPlayer`:

1. Met kans 0,2 (`DefaultExplorationRate`) speelt hij een willekeurige zet, zodat hij zelf niet voorspelbaar wordt. De predictor wordt dan niet eens gevraagd.
2. Anders vraagt hij de predictor om een voorspelling en speelt hij een zet uit `Rules.CountersOf(voorspelling)`.
3. Zonder voorspelling speelt hij willekeurig.

Ik koos compositie (een predictor meegeven) boven een subklasse per tactiek: minder klassen, en elke predictor is los te testen. `Random` wordt geïnjecteerd: geseed of nep in tests, één per spel in de echte app.

`RandomComputerPlayer` gokt altijd. Dat is het speltheoretische evenwicht: niet te verslaan, maar wint gemiddeld ook niets.

### Resultaten (simulatietests, 1000 rondes, geseed)

| Tegenstander | Speler | Computer wint | Verliest | Gelijk |
|---|---|---|---|---|
| Vorige zet | vaste cyclus (rots, papier, schaar, ...) | 86,8% | 6,4% | 6,8% |
| Zet + uitkomst | win-stay, lose-shift | 86,6% | 7,3% | 6,1% |
| elke tegenstander | willekeurig | 31,6–36,3% | 30,5–34,9% | 32,5–35,0% |

Het plafond tegen een voorspelbare speler is ongeveer 87%: 80% van de rondes speelt de computer de counter, en de 20% exploratierondes wint hij maar voor een derde. De tegenstanders zitten daar tegen het patroon waarvoor ze gemaakt zijn vrijwel tegenaan. Tegen een willekeurige speler valt niets te voorspellen, en dat klopt ook.

## Uitbreidbaar: meer zetten

Rots-papier-schaar is een *intransitief* spel: elke zet wint van precies de helft van de andere. Dat werkt met elk oneven aantal zetten, bijvoorbeeld vijf met Vuur en Water:

| Wint van → | Rock | Paper | Scissors | Fire | Water |
|---|---|---|---|---|---|
| **Rock** | | | ✓ | ✓ | |
| **Paper** | ✓ | | | | ✓ |
| **Scissors** | | ✓ | | | ✓ |
| **Fire** | | ✓ | ✓ | | |
| **Water** | ✓ | | | ✓ | |

Dit heb ik niet gebouwd, maar uitbreiden raakt alleen deze plekken:
1. `Move` krijgt nieuwe waarden.
2. De tabel in `Rules` krijgt de nieuwe regels.
3. De vertaling van invoer en weergave: `MoveText` (console) en `MoveNames` (web).

Al het andere werkt al voor elk aantal zetten. Nergens staat het getal 3 of een vaste lijst zetten: alles gebruikt `Enum.GetValues<Move>()`. `CountersOf` geeft een lijst, omdat er bij vijf zetten twee counters zijn (de computer kiest daar willekeurig uit). Tests vangen een vergeten stap af: de balanstest faalt bij een scheve tabel, en de vertaaltests falen als een zet geen naam heeft.

## Tests

117 xUnit-tests, allemaal deterministisch (geseede `Random` of nep-implementaties, nooit afhankelijk van toeval of tijd). Naamgeving: `Method_Situation_ExpectedResult`.

- **Rules:** alle 9 combinaties, `CountersOf`, balans en symmetrie.
- **PatternCounter en predictors:** juiste voorspelling op handgemaakte histories, `null` bij een lege history en bij een gelijke stand.
- **RandomComputerPlayer:** kiest uit alle zetten.
- **PredictingComputerPlayer** (met een nep-predictor): speelt de counter, speelt willekeurig zonder voorspelling, exploreert bij een lage worp zonder de predictor te vragen, en geeft de predictor precies de ontvangen history.
- **Opponents:** elke optie maakt een werkende computerspeler, de standaard staat in de lijst, namen zijn uniek.
- **GameSession:** klaar na precies n rondes, score klopt met de history, weigert een ronde na afloop, en eerlijkheid.
- **Console:** het hele spelverloop via `StringReader`/`StringWriter`, inclusief de keuze van tegenstander, ongeldige invoer en einde van de invoer.
- **Simulaties:** zie de resultaten hierboven.

De console-I/O zit achter `TextReader`/`TextWriter`, dus het spelverloop is te testen zonder echte console. De webpagina heeft geen eigen tests: die zou een extra package vragen (bUnit), en de pagina bevat geen logica die niet al in Core getest is.

## Hoe het ontwerp zich ontwikkelde

De eerste versie had één computerspeler met alle predictors tegelijk (een *ensemble*, *prediction with expert advice*, geïnspireerd op "Iocaine Powder" uit de RPS-programmeerwedstrijden). Elke ronde volgde hij de predictor die tot dan toe het vaakst gelijk had.

Om die trefferscores te berekenen, speelde hij eerst de history na: voor elke predictor en elke eerdere ronde opnieuw voorspellen. Dat kost O(P·r²) per ronde en O(P·n³) per spel. Gemeten duurde één simulatie van 1000 rondes ~20 s en de testsuite ~25 s. Ik heb dat opgelost door elke ronde de voorspellingen van alle predictors vast te leggen in de ronde zelf: de score werd een telling, de suite ging naar ~0,5 s, met aantoonbaar identieke resultaten.

Dat werkte, maar het maakte het ontwerp zwaarder: een ronde was niet meer alleen twee zetten, voorspellingen waren op positie gekoppeld aan de predictors (een history van een andere configuratie moest worden geweigerd), en eerlijkheid betekende ook "voorspellingen liggen vast voordat de zet bekend is".

Daarom heb ik het ensemble weer weggehaald en vervangen door vaste tegenstanders met elk één predictor. Wat er verdween: `ComputerPlayer`, `ComputerDecision`, `Round.Predictions` met eigen gelijkheid, de trefferscore en de controle op de configuratie. Wat er terugkwam is eenvoudiger: een lijst tegenstanders en een keuzevraag. Het snelheidsprobleem bestaat niet meer, omdat de oorzaak weg is. De computer is per tegenstander wel minder slim: hij schakelt niet om als de speler van tactiek wisselt. Voor een opdracht die op codekwaliteit wordt beoordeeld, weegt dat niet op tegen de complexiteit.

## Wat ik bewust niet heb gedaan, en waarom

### Adaptieve tegenstander

Een `AdaptiveComputerPlayer` die zelf de beste predictor kiest, zoals het ensemble hierboven, past als extra `IComputerPlayer` en extra regel in `Opponents`, zonder iets anders te wijzigen. Voor de trefferscores zijn er twee bewezen routes: één doorloop per predictor met tijdelijke tellingen (*sweep*, O(r) per ronde en volledig stateless), of de voorspellingen vastleggen in de history (nodig bij een externe predictor). *Decay*, recente rondes zwaarder laten wegen, zou hem sneller laten omschakelen. Niet gebouwd om de reden hierboven.

### Persistente opslag van de history

`IHistoryStore` met een `InMemoryHistoryStore` is er als haakje, maar is nergens aangesloten. Omdat de history alles is, zou opslag eenvoudig zijn: bewaar de rondes en de rest is terug te rekenen.

- **Voordeel:** meer history per speler, dus eerder goede voorspellingen.
- **Nadeel:** spelers gedragen zich per sessie anders, dus oude history voorspelt minder dan je zou verwachten.
- **Kosten:** gebruikers identificeren, een opslag kiezen (JSON-bestand, SQLite), en de AVG, want je slaat gedragsgegevens van personen op.

### AI-tegenstander

Een predictor die een taalmodel (zoals Claude, met *structured output*) om een voorspelling vraagt, past achter `IPredictor` en wordt een tegenstander via `PredictingComputerPlayer`: alleen de history meesturen, `null` bij fouten of een time-out, zodat de computer dan willekeurig speelt. Hij wordt hooguit één keer per ronde aangeroepen, en niet in exploratierondes. Ik heb hem niet ingebouwd, omdat de beoordelaar het spel moet kunnen draaien zonder API-sleutel of internet.

Wat nodig zou zijn:

- **Async door alle lagen.** `ValueTask<Move?> PredictAsync(history, CancellationToken)` werkt door naar `IComputerPlayer`, `GameSession`, de console-lus en de tests. Mechanisch, maar overal.
- **Een eigen project.** Core doet geen netwerk-I/O, dus de predictor komt in bijvoorbeeld `RockPaperScissors.Ai`. `Opponents.All` in Core kan hem niet kennen; de composition root voegt de AI-tegenstander aan de lijst toe als er een API-sleutel is, en Console en Web tonen die uitgebreide lijst.
