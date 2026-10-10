# Rots, papier, schaar

Een C#-programma waarin je Rots-Papier-Schaar speelt tegen de computer, als console-app en als webapp. Je kiest bij de start hoeveel rondes je speelt. De computer probeert patronen in je spel te herkennen, maar speelt aantoonbaar eerlijk.

De ontwerpspecificatie staat in [DESIGN.md](DESIGN.md). Deze README legt de keuzes uit.

## Starten

Vereist: .NET 8 SDK. Een nieuwere SDK werkt ook; [global.json](global.json) rolt dan door.

```
dotnet build
dotnet test
dotnet run --project src/RockPaperScissors.Console
dotnet run --project src/RockPaperScissors.Web
```

In de console typ je `r`, `p` of `s`, of de hele naam. Hoofdletters maken niet uit. Ctrl+Z (Windows) of Ctrl+D (Linux/macOS) stopt het spel netjes. De webapp draait op het adres dat `dotnet run` toont (standaard http://localhost:5013).

## Structuur

```
src/
  RockPaperScissors.Core/      alle spellogica, geen I/O, geen verwijzingen naar andere projecten
    Prediction/                contexten, tabel, predictors, ensemble
    History/                   het haakje voor historie-opslag
  RockPaperScissors.Console/   console-interface en composition root
  RockPaperScissors.Web/       Blazor Server-interface en composition root
tests/
  RockPaperScissors.Tests/     xUnit
```

Afhankelijkheden lopen alleen naar `Core`. Console en web zijn dunne lagen: invoer lezen, `GameSession` aanroepen, resultaat tonen. Dat er twee interfaces op dezelfde `Core` draaien, laat zien dat de scheiding echt is.

Er is bewust geen volledige Clean Architecture met aparte Application- en Infrastructure-lagen. Voor deze omvang laten `Core` en twee interfaces hetzelfde principe zien, zonder lagen die alleen doorgeven.

## Ontwerpkeuzes

### Regels zijn data

Wie wint staat op één plek: de uitkomstmatrix in [Rules](src/RockPaperScissors.Core/Rules.cs). Er is geen if-else-keten. `CounterOf` (de zet die een andere zet verslaat) wordt uit dezelfde matrix afgeleid, zodat de twee het nooit oneens kunnen zijn. Een test controleert ook de symmetrie van de matrix: als A wint van B, verliest B van A.

### De computer speelt eerlijk, en dat is afdwingbaar

- Een strategie krijgt alleen afgeronde rondes ([IStrategy](src/RockPaperScissors.Core/IStrategy.cs)). De zet van de speler in de huidige ronde kan haar dus niet bereiken.
- De computer legt zijn zet **vooraf** vast. [GameSession](src/RockPaperScissors.Core/GameSession.cs) laat de strategie kiezen bij het aanmaken van de sessie en direct na elke ronde. Die zet blijft privé. Zo heeft de computer gekozen voordat een interface de speler iets vraagt.
- De historie die de strategie krijgt is alleen-lezen. Ook eerdere rondes kunnen niet worden herschreven.
- Tests bewijzen dit. Een spionstrategie laat zien dat ze in ronde k precies de k−1 eerdere rondes ziet. Een nep-console legt de volgorde van gebeurtenissen vast: steeds eerst "computer kiest", daarna "invoer lezen".

### Patroonherkenning: context, tabel, beslisser

De slimme computer bestaat uit losse onderdelen, elk met één verantwoordelijkheid:

| Onderdeel | Rol |
|---|---|
| Context ([IContext](src/RockPaperScissors.Core/Prediction/IContext.cs)) | De **hypothese**: zet de historie om in een sleutel, bijvoorbeeld "vorige zet + uitkomst". |
| [TransitionTable](src/RockPaperScissors.Core/Prediction/TransitionTable.cs) | Het **geheugen**: telt per sleutel welke zet volgde. |
| [Predictor](src/RockPaperScissors.Core/Prediction/Predictor.cs) | Koppelt één context aan een eigen tabel en verbergt het sleuteltype. |
| [EnsemblePredictor](src/RockPaperScissors.Core/Prediction/EnsemblePredictor.cs) | Volgt de predictor die de laatste tijd het vaakst gelijk had. |
| [PatternStrategy](src/RockPaperScissors.Core/PatternStrategy.cs) | De **beslisser**: speelt de counter van de voorspelling, deels random. |

Enkele details:

- **Drie contexten:**
  - vorige zet: vangt vaste cycli;
  - vorige zet plus uitkomst: vangt win-stay, lose-shift, bij mensen gemiddeld het meest voorspellend;
  - laatste twee zetten: vangt langere reeksen.
- **Sleutels zijn `readonly record struct`'s.** Ze worden op inhoud vergeleken en de compiler voorkomt dat sleutels van verschillende contexten door elkaar raken. Er zijn geen string-sleutels.
- **Laplace-smoothing:** elke teller begint op 1, zodat één waarneming nog geen harde conclusie is. Bij gelijke stand kiest de tabel random uit de koplopers, anders zou een onbekende situatie altijd "rots" voorspellen.
- **Ensemble met decay:** een predictor krijgt +1 als hij goed voorspelde, en alle scores worden elke ronde × 0,9 vermenigvuldigd. Wisselt de speler van tactiek, dan schakelt de computer binnen enkele rondes om. Bij gelijke score wint de eenvoudigste context. Die heeft in het begin de meeste data, wat het probleem van te weinig data (*data sparsity*) oplost.
- **Exploratie:** in 20% van de rondes speelt de computer random, ook als hij een voorspelling heeft. Zo wordt hij zelf niet voorspelbaar. De exploratiekans en de decay zijn benoemde, configureerbare waarden (`PatternStrategyOptions`, `EnsembleOptions`).
- **Het ensemble is zelf een predictor**, geen aparte strategie. "Voorspelling → counter → exploratie" staat daardoor maar op één plek. [StrategyFactory](src/RockPaperScissors.Core/StrategyFactory.cs) stelt de standaard-computer samen, zodat console en web hem identiek opbouwen.

### Leren uit de historie

Een strategie heeft alleen `ChooseMove(history)`; er is geen aparte `Learn`-aanroep. `PatternStrategy` speelt bij elke beurt de rondes na die ze nog niet gezien heeft, met precies de historie die eraan voorafging. De sessie kan het leren dus niet vergeten of in de verkeerde volgorde aanroepen. De historie mag alleen groeien; krimpt ze, dan volgt een exceptie.

### Testbaar zonder console of browser

- `GameSession` heeft geen lus en doet geen I/O.
- [ConsoleGame](src/RockPaperScissors.Console/ConsoleGame.cs) leest en schrijft via `TextReader`/`TextWriter`, dus het hele spelverloop is te testen met een nep-console.
- `Random` wordt overal geïnjecteerd. Tests gebruiken een vaste seed of een nep-implementatie, zodat ze nooit van toeval afhangen.

### Uitbreiden door toevoegen

- **Een nieuwe tactiek** is een nieuwe klasse die `IStrategy` implementeert.
- **Een nieuwe hypothese over de speler** bestaat uit een nieuwe sleutel, een nieuwe context en één regel in `StrategyFactory`. De tabel, predictor en het ensemble blijven ongewijzigd.
- **Rots-Papier-Schaar-Hagedis-Spock** betekent de `Move`-enum en de matrix uitbreiden, plus de teksten in de interfaces. Eén kanttekening: in die variant verslaan twee zetten elke zet. `CounterOf` kiest dan de eerste. Wil je beide benutten, dan is dat een kleine uitbreiding van `Rules`.

### Randgevallen

- **Ongeldige zet of ongeldig aantal rondes:** het programma vraagt opnieuw. Een geldig aantal ligt tussen 1 en 1000 (`GameSession.MinRounds`/`MaxRounds`, een domeinregel die beide interfaces gebruiken).
- **Gelijkspel** telt als ronde en wordt apart bijgehouden.
- **Einde van de invoer** sluit netjes af, met de stand tot dan toe.
- **Een ronde na afloop** geeft in `Core` een duidelijke exceptie. In de webapp zijn de knoppen dan uitgeschakeld.
- **Invoer via een pipe in Windows PowerShell:** die zet een byte order mark vóór de invoer. De console-app leest omgeleide invoer daarom met een reader die dat teken herkent.

## Wat ik bewust niet heb gedaan, en waarom

- **Persistente historie-opslag.** Er is alleen een haakje ([IHistoryStore](src/RockPaperScissors.Core/History/IHistoryStore.cs)) met een in-memory implementatie, die niet op het spel is aangesloten. Opslag in een JSON-bestand of SQLite zou achter dat haakje passen.
  - *Voordeel:* meer historie per speler, dus eerder goede voorspellingen.
  - *Nadeel:* spelers gedragen zich per sessie anders, dus oude historie voorspelt minder dan je zou verwachten.
  - *Kosten:* je moet spelers identificeren, een opslag kiezen en beheren, en je slaat gedragsgegevens van personen op. Dat valt onder de AVG (grondslag, bewaartermijn, inzage- en verwijderrecht). Voor een spel weegt dat niet op tegen de winst.
- **Een AI-strategie** (bijvoorbeeld Claude of OpenAI met gestructureerde output). Die zou achter `IStrategy` passen:
  - alleen de historie meesturen;
  - terugvallen op random bij een fout of time-out;
  - de API-sleutel uit configuratie halen.

  Ik heb hem niet gebouwd, omdat de beoordelaar het spel zonder sleutel en zonder internet moet kunnen draaien. Een taalmodel is bovendien trager en minder voorspelbaar dan de tellende aanpak.
- **Terugvallen naar een eenvoudigere context bij een lege rij** (*backoff*). Het ensemble lost hetzelfde probleem grotendeels op, door de eenvoudige context te volgen zolang de complexe nog weinig data heeft.
- **Geautomatiseerde tests van de webpagina** (bijvoorbeeld met bUnit). Dat zou een extra package zijn, en de pagina bevat geen logica: alles wat getest moet worden zit in `Core` en is daar getest.
