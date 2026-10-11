# Rots, papier, schaar

Een C#-programma dat rots-papier-schaar speelt tussen de computer en een echte speler. De speler kiest bij de start het aantal rondes (1 tot en met 1000). Er zijn twee interfaces op dezelfde spellogica: een console-app en een minimale Blazor Server-webapp.

De computer gokt niet alleen. Hij zoekt patronen in wat de speler eerder deed en speelt de zet die daartegen wint. Tegen een speler met een vast patroon wint hij ongeveer 86% van de rondes; tegen een willekeurige speler blijft het rond een derde.

## Draaien en testen

Vereist: .NET 10 SDK (vastgelegd in `global.json`).

```
dotnet build
dotnet test
dotnet run --project src/RockPaperScissors.Console
dotnet run --project src/RockPaperScissors.Web
```

- **Console:** zetten invoeren als `r`/`p`/`s` of `rots`/`papier`/`schaar`, hoofdletters maken niet uit. Ongeldige invoer leidt tot een nieuwe vraag. Ctrl+Z (Windows) of Ctrl+D (Linux/macOS) sluit netjes af.
- **Web:** open de URL die in de console verschijnt (standaard `http://localhost:5256`).
- **Tests:** de volledige suite duurt zo'n 30 seconden, bijna helemaal door drie simulaties van 1000 rondes (zie [De prijs van niets opslaan](#de-prijs-van-niets-opslaan)). Zonder simulaties draait hij in minder dan een seconde: `dotnet test --filter Category!=Simulation`.

## Structuur

```
src/
  RockPaperScissors.Core/      spellogica: domein, Rules, predictors, ComputerPlayer, GameSession (geen I/O)
  RockPaperScissors.Console/   console-interface en composition root
  RockPaperScissors.Web/       Blazor Server-interface en composition root
tests/
  RockPaperScissors.Tests/     xUnit
```

Afhankelijkheden lopen alleen naar `Core`; `Core` kent niets anders. Console en Web lezen invoer, roepen `GameSession` aan en tonen het resultaat. Ze bevatten geen spelregels. Dat er twee interfaces zijn, is het bewijs dat die scheiding werkt.

Ik heb bewust geen volledige Clean Architecture met aparte Application- en Infrastructure-lagen gebouwd. Voor deze omvang laten Core plus twee interfaces hetzelfde principe zien zonder overdaad.

## Kernidee: de history is de enige toestand

De lijst gespeelde rondes is het enige wat het spel bijhoudt. Al het andere wordt er op het moment zelf uit berekend:

| Gegeven | Afgeleid uit de history |
|---|---|
| Uitkomst van een ronde | `Round.Outcome` past de regels toe op de twee zetten |
| Score | uitkomsten tellen |
| Voorspelling van een predictor | patroon zoeken in de history |
| Welke predictor het beste is | elke predictor de history laten naspelen en treffers tellen |

Er zijn geen tellers, tabellen, caches of `Learn`-methodes. Predictors en `ComputerPlayer` hebben geen velden die tijdens het spel veranderen; elke beslissing is een pure functie van de history (plus een random getal voor exploratie).

Waarom:
- **Eén bron van waarheid.** Afgeleide gegevens kunnen nooit uit de pas lopen met de history.
- **Eenvoudig testen.** Geef een vaste history, controleer de uitkomst. Je hoeft geen interne toestand op te bouwen.
- **Minder code en minder begrippen** dan een opzet met lerende tabellen.
- **Opslaan is triviaal:** wie de history heeft, heeft alles.

Het idee lijkt op *event sourcing*: de lijst gebeurtenissen is de waarheid, toestand wordt eruit afgeleid.

### De prijs van niets opslaan

De computer rekent elke ronde alles opnieuw uit. Om de beste predictor te kiezen speelt hij voor elke predictor de hele history na. Per ronde is dat O(n²), over een heel spel O(n³).

| | Tijd |
|---|---|
| Simulatie van 1000 rondes in Debug (gemeten) | ~20 s |
| Simulatie van 1000 rondes in Release (gemeten) | ~7 s |
| Duurste ronde in een spel, ronde 1000 (afgeleid: kosten groeien met r², dus de laatste ronde kost ongeveer 3/n van het totaal) | ~60 ms in Debug |

Voor een mens die speelt is dat onmerkbaar. Het maximum van 1000 rondes houdt het begrensd. Alleen de simulatietests merken het; die staan daarom elk in een eigen testklasse, zodat xUnit ze parallel draait.

Eén optimalisatie heb ik geprobeerd en weer teruggedraaid: `PatternCounter` herschreven van LINQ naar lussen. Dat leverde gemeten niets op, want de tijd zit in het naspelen zelf, niet in allocaties. De eenvoudigere versie is gebleven. De echte versnelling zou caching van trefferscores zijn; zie [Wat ik bewust niet heb gedaan](#wat-ik-bewust-niet-heb-gedaan-en-waarom).

## Spelregels: één tabel

`Rules` is de enige plek die weet wie wint. De regels staan in een tabel "wint van" (`Rock → {Scissors}`, ...). `Resolve` en `CountersOf` leiden alles daaruit af. Nergens anders staat een `if` of `switch` op zetcombinaties.

Ik koos een "wint van"-tabel in plaats van een volledige 3x3-uitkomstmatrix omdat elk paar er dan maar één keer in staat. Een volledige matrix kan zichzelf tegenspreken ("rots verliest van papier" én "papier verliest van rots"); deze tabel niet. Een test bewaakt bovendien dat de regels gebalanceerd zijn: elke zet wint van precies evenveel zetten als hij van verliest.

## Eerlijkheid

De computer kiest zijn zet voordat de zet van de speler verwerkt wordt, en krijgt alleen afgeronde rondes te zien. Dat is niet alleen een afspraak, het is afgedwongen en getest:

- `GameSession.PlayRound` roept eerst `computer.ChooseMove(History)` aan en voegt pas daarna de ronde toe. De zet van de speler kan de computer niet bereiken.
- De history gaat naar buiten als read-only weergave.
- Bij het naspelen voor de trefferscore ziet een predictor bij ronde *i* alleen de rondes daarvóór, via een kleine `HistoryPrefix`-weergave zonder kopie. Kijken voorbij dat prefix geeft een exceptie.
- Tests met een nep-computerspeler en nep-predictors leggen vast wat ze ontvangen, en controleren dat het precies de afgeronde rondes zijn.

## Hoe de computer kiest

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

### Ensemble: de beste predictor volgen

`ComputerPlayer` geeft elke predictor een **trefferscore**: hoe vaak hij gelijk had gehad als we hem vanaf het begin hadden gevolgd. De predictor met de hoogste score wint; bij gelijke score de eerste in de lijst. Daarna speelt de computer een zet die de voorspelling verslaat.

In het begin wint zo vanzelf een eenvoudige predictor (die heeft het snelst genoeg data), en later neemt een specifiekere het over als die vaker gelijk heeft. Dat lost het *data sparsity*-probleem op zonder extra code. Het principe heet *prediction with expert advice*; de inspiratie komt uit de RPS-programmeerwedstrijden ("Iocaine Powder", Dan Egnor, 1999).

### Exploratie

Met kans 0,2 (`ComputerPlayer.DefaultExplorationRate`) speelt de computer een willekeurige zet, zodat hij zelf niet voorspelbaar wordt. Zonder predictors of zonder voorspelling speelt hij ook willekeurig, dus een aparte `RandomStrategy` is niet nodig. `Random` wordt geïnjecteerd: geseed of nep in tests, één per sessie in de echte app.

### Resultaten (simulatietests, 1000 rondes, geseed)

| Tegenstander | Computer wint | Verliest | Gelijk |
|---|---|---|---|
| Vaste cyclus (rots, papier, schaar, ...) | 86,7% | 6,4% | 6,9% |
| Win-stay, lose-shift | 86,0% | 7,6% | 6,4% |
| Willekeurig | 35,3% | 31,8% | 32,9% |

Het plafond tegen een voorspelbare speler is ongeveer 87%: 80% van de rondes speelt de computer de counter, en de 20% exploratierondes wint hij maar voor een derde. Tegen de twee patronen zit hij daar dus vrijwel tegenaan. Tegen een willekeurige speler valt niets te voorspellen, en dat klopt ook.

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

104 xUnit-tests, allemaal deterministisch (geseede `Random` of nep-implementaties, nooit afhankelijk van toeval of tijd). Naamgeving: `Method_Situation_ExpectedResult`.

- **Rules:** alle 9 combinaties, `CountersOf`, balans en symmetrie.
- **PatternCounter en predictors:** juiste voorspelling op handgemaakte histories, `null` bij een lege history en bij een gelijke stand.
- **ComputerPlayer:** volgt de beste predictor (met nep-predictors), speelt de counter, speelt willekeurig zonder voorspelling, exploratie, en eerlijk naspelen.
- **GameSession:** klaar na precies n rondes, score klopt met de history, weigert een ronde na afloop, eerlijkheid.
- **Console:** het hele spelverloop via `StringReader`/`StringWriter`, inclusief ongeldige invoer en einde van de invoer.
- **Simulaties:** zie de resultaten hierboven.

De console-I/O zit achter `TextReader`/`TextWriter`, dus het spelverloop is te testen zonder echte console. De webpagina heeft geen eigen tests: die zou een extra package vragen (bUnit), en de pagina bevat geen logica die niet al in Core getest is.

## Wat ik bewust niet heb gedaan, en waarom

### Persistente opslag van de history

`IHistoryStore` met een `InMemoryHistoryStore` is er als haakje, maar is nergens aangesloten. Omdat de history alles is, zou opslag eenvoudig zijn: bewaar de rondes en de rest is terug te rekenen.

- **Voordeel:** meer history per speler, dus eerder goede voorspellingen.
- **Nadeel:** spelers gedragen zich per sessie anders, dus oude history voorspelt minder dan je zou verwachten.
- **Kosten:** gebruikers identificeren, een opslag kiezen (JSON-bestand, SQLite), en de AVG, want je slaat gedragsgegevens van personen op.

### AI-predictor

Een predictor die een taalmodel (zoals Claude, met *structured output*) om een voorspelling vraagt, past achter `IPredictor`: alleen de history meesturen, `null` bij fouten. Ik heb hem niet standaard ingebouwd, omdat de beoordelaar het spel moet kunnen draaien zonder API-sleutel of internet. Twee gevolgen als je het wel zou doen:

- **Async door alle lagen.** `ValueTask<Move?> PredictAsync(history, CancellationToken)` werkt door naar `ComputerPlayer`, `GameSession`, de console-lus en de tests. Mechanisch, maar overal.
- **Voorspellingen vastleggen in plaats van naspelen.** Naspelen kost bij een externe predictor één aanroep per eerdere ronde, en dat elke ronde opnieuw. De oplossing die bij het kernidee past: elke `Round` legt vast wat elke predictor voorspelde, en de trefferscore is dan gewoon tellen in de history.

### Recente rondes zwaarder laten wegen

Met *decay* in de trefferscore schakelt de computer sneller om als de speler van tactiek wisselt. Dat is een kleine aanpassing in `ComputerPlayer.HitScore`, maar het voegt een parameter toe die je moet afstellen. De huidige versie past zich al aan, alleen trager.

### Caching van trefferscores

Daarmee wordt het naspelen per ronde O(n) in plaats van O(n²). Binnen het maximum van 1000 rondes is het niet nodig (zie de gemeten tijden), en het zou het kernidee doorbreken: dan staan er toch weer afgeleide gegevens naast de history die uit de pas kunnen lopen.

## Afwijking van de oorspronkelijke specificatie

De specificatie noemde .NET 8. Op verzoek is dit .NET 10 geworden. Dat is net als .NET 8 een LTS-versie, met ondersteuning tot november 2028. Core, Console en de tests gebruiken niets wat specifiek voor .NET 10 is. De webapp is gemaakt met de Blazor-template van .NET 10 en gebruikt daar onderdelen van (bijv. `MapStaticAssets`).
