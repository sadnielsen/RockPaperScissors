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
- **Tests:** de volledige suite, inclusief drie simulaties van 1000 rondes, draait in ongeveer een halve seconde.

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
| Welke predictor het beste is | tellen hoe vaak de vastgelegde voorspelling van elke predictor klopte |

Elke ronde bevat naast de twee zetten ook wat elke predictor vóór die ronde voorspelde (`Round.Predictions`). Dat is een feit van dat moment, net als de zet van de computer: hij wordt eenmalig vastgelegd en verandert nooit meer. Waarom dat zo is, staat in [Voorspellingen vastleggen in plaats van naspelen](#voorspellingen-vastleggen-in-plaats-van-naspelen).

Er zijn geen tellers, tabellen, caches of `Learn`-methodes. Predictors en `ComputerPlayer` hebben geen velden die tijdens het spel veranderen; elke beslissing is een pure functie van de history (plus een random getal voor exploratie).

Waarom:
- **Eén bron van waarheid.** Afgeleide gegevens kunnen nooit uit de pas lopen met de history.
- **Eenvoudig testen.** Geef een vaste history, controleer de uitkomst. Je hoeft geen interne toestand op te bouwen.
- **Minder code en minder begrippen** dan een opzet met lerende tabellen.
- **Opslaan is triviaal:** wie de history heeft, heeft alles.

Het idee lijkt op *event sourcing*: de lijst gebeurtenissen is de waarheid, toestand wordt eruit afgeleid.

### Voorspellingen vastleggen in plaats van naspelen

**De eerste versie** berekende de trefferscore door de history na te spelen: voor elke predictor en elke eerdere ronde i opnieuw `Predict(rondes vóór i)`. Elke voorspelling loopt zelf ook de history door, dus dat kost O(P·r²) per ronde en O(P·n³) per spel (P = aantal predictors).

De verspilling: de history groeit alleen aan het eind, dus de voorspelling voor een eerdere ronde is elke ronde precies dezelfde. Toch werd hij telkens opnieuw berekend; van het werk per ronde was maar O(r) nieuw.

**Was dat een probleem?** Voor één speler niet: de zwaarste ronde kostte ~60 ms. Wel in deze gevallen:
- **Tests:** een simulatie van 1000 rondes duurde ~20 s, de hele suite ~25 s. Dat is een trage feedbacklus.
- **De webserver:** bij Blazor Server rekent de server. Veel gelijktijdige spelers tellen op.
- **Langere history:** met persistente opslag groeit de history over sessies, en de kosten per ronde groeien kwadratisch mee.
- **Een externe of AI-predictor:** naspelen kost dan één API-aanroep per eerdere ronde, en dat elke ronde opnieuw. Dat is onbruikbaar.

**De oplossing:** de computer legt per ronde vast wat elke predictor voorspelde, voordat de zet van de speler bekend is (`ComputerDecision`, opgeslagen in `Round.Predictions`). De trefferscore is daarna een telling in de history. Elke predictor wordt per ronde nog maar één keer aangeroepen. Per ronde kost dat O(P·r).

| | Naspelen (eerst) | Vastgelegd (nu) |
|---|---|---|
| Simulatie van 1000 rondes (Debug, gemeten) | ~20 s | ~0,2 s |
| Hele testsuite (gemeten) | ~25 s | ~0,5 s |
| Simulatieresultaten | 867 / 860 / 353 gewonnen | identiek |

Dat de resultaten identiek zijn, is geen toeval. De computer gebruikt zijn random getallen in dezelfde volgorde als eerst, en de vastgelegde voorspellingen zijn precies wat het naspelen opleverde. Het gedrag is dus aantoonbaar onveranderd.

**Waarom dit binnen het kernidee past:** een voorspelling is wat de computer op dat moment verwachtte, een gebeurtenis zoals zijn zet. Ze staat ín de ronde en is onveranderlijk, dus ze kan niet uit de pas lopen met de history. Dat is het verschil met een cache, die ernaast staat en moet worden bijgewerkt.

**Wat het kost:**
- `Round` is gekoppeld aan de predictors van de computer: voorspellingen staan op positie, in de volgorde van de predictors. Een history van een computer met andere predictors wordt geweigerd (`ArgumentException`), in plaats van stil verkeerde scores te geven.
- Een nieuwe predictor heeft geen verleden en begint dus op score 0. Bij naspelen zou hij meteen een score over de hele history hebben.

**Overwogen en afgevallen:**
- **Micro-optimalisatie.** `PatternCounter` met lussen in plaats van LINQ leverde gemeten niets op, en een Release-build is ~3× sneller. Beide laten de kubische groei bestaan.
- **Eén doorloop per predictor (sweep).** Elke patroon-predictor is te schrijven als "situatie vóór ronde j". Eén doorloop met lokale tellingen geeft dan alle voorspellingen en treffers in O(r), zonder iets op te slaan. Even snel en volledig stateless, maar de predictors krijgen een andere vorm, en het werkt niet voor een externe predictor. Vastleggen lost beide problemen in één keer op.
- **Een cache in `ComputerPlayer`.** Die maakt de computer stateful, en geeft hier een concrete bug: in de webapp wordt dezelfde `ComputerPlayer` per circuit hergebruikt over meerdere spellen, dus na "Start spel" zou hij scores van het vorige spel gebruiken.
- **Alleen de laatste W rondes meetellen.** Dat verandert het gedrag (het is eigenlijk *decay*) en wint weinig, omdat elke voorspelling nog steeds de hele history doorloopt.
- **Predictors parallel uitrekenen.** Hooguit een constante factor, op een webserver vaak niets, en het voegt concurrency toe.

## Spelregels: één tabel

`Rules` is de enige plek die weet wie wint. De regels staan in een tabel "wint van" (`Rock → {Scissors}`, ...). `Resolve` en `CountersOf` leiden alles daaruit af. Nergens anders staat een `if` of `switch` op zetcombinaties.

Ik koos een "wint van"-tabel in plaats van een volledige 3x3-uitkomstmatrix omdat elk paar er dan maar één keer in staat. Een volledige matrix kan zichzelf tegenspreken ("rots verliest van papier" én "papier verliest van rots"); deze tabel niet. Een test bewaakt bovendien dat de regels gebalanceerd zijn: elke zet wint van precies evenveel zetten als hij van verliest.

## Eerlijkheid

De computer kiest zijn zet voordat de zet van de speler verwerkt wordt, en krijgt alleen afgeronde rondes te zien. Dat is niet alleen een afspraak, het is afgedwongen en getest:

- `GameSession.PlayRound` roept eerst `computer.ChooseMove(History)` aan en voegt pas daarna de ronde toe. De zet van de speler kan de computer niet bereiken.
- De voorspellingen worden in dezelfde aanroep gemaakt, dus ook zij liggen vast voordat de zet van de speler gebruikt wordt. De trefferscores tellen alleen die vooraf gemaakte voorspellingen.
- De history gaat naar buiten als read-only weergave.
- Tests met een nep-computerspeler en nep-predictors leggen vast wat ze ontvangen. Ze controleren dat het precies de afgeronde rondes zijn, en dat de voorspelling in ronde k is gemaakt op precies de k−1 rondes ervoor.

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

Elke ronde vraagt `ComputerPlayer` alle predictors om een voorspelling en legt die vast. De **trefferscore** van een predictor is hoe vaak zijn vastgelegde voorspelling klopte. De predictor met de hoogste score wint; bij gelijke score de eerste in de lijst. Daarna speelt de computer een zet die de voorspelling verslaat.

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

108 xUnit-tests, allemaal deterministisch (geseede `Random` of nep-implementaties, nooit afhankelijk van toeval of tijd). Naamgeving: `Method_Situation_ExpectedResult`.

- **Rules:** alle 9 combinaties, `CountersOf`, balans en symmetrie.
- **PatternCounter en predictors:** juiste voorspelling op handgemaakte histories, `null` bij een lege history en bij een gelijke stand.
- **ComputerPlayer:** volgt de predictor met de meeste vastgelegde treffers (met nep-predictors), speelt de counter, speelt willekeurig zonder voorspelling, legt ook bij exploratie voorspellingen vast, en weigert een history van een andere configuratie.
- **Round:** waarde-gelijkheid, ook over de voorspellingen.
- **GameSession:** klaar na precies n rondes, score klopt met de history, weigert een ronde na afloop, en eerlijkheid van zet én voorspellingen.
- **Console:** het hele spelverloop via `StringReader`/`StringWriter`, inclusief ongeldige invoer en einde van de invoer.
- **Simulaties:** zie de resultaten hierboven.

De console-I/O zit achter `TextReader`/`TextWriter`, dus het spelverloop is te testen zonder echte console. De webpagina heeft geen eigen tests: die zou een extra package vragen (bUnit), en de pagina bevat geen logica die niet al in Core getest is.

## Wat ik bewust niet heb gedaan, en waarom

### Persistente opslag van de history

`IHistoryStore` met een `InMemoryHistoryStore` is er als haakje, maar is nergens aangesloten. Omdat de history alles is, zou opslag eenvoudig zijn: bewaar de rondes en de rest is terug te rekenen.

- **Voordeel:** meer history per speler, dus eerder goede voorspellingen.
- **Nadeel:** spelers gedragen zich per sessie anders, dus oude history voorspelt minder dan je zou verwachten.
- **Nadeel:** opgeslagen rondes bevatten de voorspellingen van de predictors van dat moment. Voeg je later een predictor toe, dan past een oude history niet meer. Je moet hem dan migreren, of de voorspellingen per predictor-naam vastleggen in plaats van op positie.
- **Kosten:** gebruikers identificeren, een opslag kiezen (JSON-bestand, SQLite), en de AVG, want je slaat gedragsgegevens van personen op.

### AI-predictor

Een predictor die een taalmodel (zoals Claude, met *structured output*) om een voorspelling vraagt, past achter `IPredictor`: alleen de history meesturen, `null` bij fouten. Ik heb hem niet standaard ingebouwd, omdat de beoordelaar het spel moet kunnen draaien zonder API-sleutel of internet.

Het grootste obstakel is al weg: omdat voorspellingen worden vastgelegd, wordt zo'n predictor per ronde één keer aangeroepen in plaats van één keer per eerdere ronde. Wat nog nodig zou zijn:

- **Async door alle lagen.** `ValueTask<Move?> PredictAsync(history, CancellationToken)` werkt door naar `ComputerPlayer`, `GameSession`, de console-lus en de tests. Mechanisch, maar overal.

### Recente rondes zwaarder laten wegen

Met *decay* in de trefferscore schakelt de computer sneller om als de speler van tactiek wisselt. Dat is een kleine aanpassing in `ComputerPlayer.HitScore`, maar het voegt een parameter toe die je moet afstellen. De huidige versie past zich al aan, alleen trager.

## Afwijkingen van de oorspronkelijke specificatie

**Voorspellingen vastleggen.** De specificatie berekende de trefferscore door de history na te spelen, en noemde vastgelegde voorspellingen alleen als toekomstige stap voor een AI-predictor. Na meting is dat naar voren gehaald; zie [Voorspellingen vastleggen in plaats van naspelen](#voorspellingen-vastleggen-in-plaats-van-naspelen). DESIGN.md is bijgewerkt.

**.NET 10.** De specificatie noemde .NET 8. Op verzoek is dit .NET 10 geworden. Dat is net als .NET 8 een LTS-versie, met ondersteuning tot november 2028. Core, Console en de tests gebruiken niets wat specifiek voor .NET 10 is. De webapp is gemaakt met de Blazor-template van .NET 10 en gebruikt daar onderdelen van (bijv. `MapStaticAssets`).
