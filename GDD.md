# Game Design Document (GDD)

## 1. Projecttitel
RaceGame

## 2. Genre
Arcade racing / auto-race game / single-player racer met AI-competitie.

## 3. Kernconcept
De speler rijdt in een 3D-racesimulatietraining met een voertuig over een vaste racebaan. Het doel is om de baan te volgen, de juiste snelheid te houden, checkpoints te passeren en de race te voltooien binnen de kortste tijd. De game combineert arcade-besturing met eenvoudige race-mechanica, HUD-feedback en AI-tegenstanders.

## 4. Doelgroep
- Casual spelers die snel kunnen instappen
- Racingfans die een compacte arcade-raceervaring willen
- Unity-project voor prototype / demo / conceptversie

## 5. Speldoel
- Kies een circuit
- Start de race na een countdown
- Volg de route via waypoints en checkpoints
- Rijd een of meerdere ronden
- Voltooi de race met de beste tijd of beste positie
- Zoveel mogelijk winnen tegen AI-auto's of de baan veilig rijden

## 6. Core loop
1. Speler opent het hoofdmenu
2. Speler kiest een baan
3. Race start na countdown
4. Speler accelereert, stuurt en remt
5. AI-auto's volgen ook de route
6. Speler passeert checkpoints en voltooit lap(s)
7. Eindresultaat wordt getoond op finish scherm
8. Speler keert terug naar menu

## 7. Gameplay-mechanica

### 7.1 Besturing
- Gas, rem, stuurinvoer via keyboard
- `Vertical (W, S)` voor accelereren/remmen
- `Horizontal(A, D)` voor sturen
- `Space` voor harde braking
- `C` voor camera wisselen
- `Escape` voor pauzeren

### 7.2 Voertuiggedrag
- Rijdende auto met WheelCollider-physics
- Snelheidslimiet per voertuig
- Versnelling, remkracht en stuurhoek zijn configureerbaar
- Auto stopt als race is afgelopen
- Auto reset bij botsing met obstakel of bij te lage snelheid op het spoor

### 7.3 AI
- AI-auto's volgen een route van waypoints
- Reageren op remzones
- Remmen wanneer snelheid te hoog is of bij braking points
- Resetten als ze van de route afglijden of een object raken

### 7.4 Route- en lap systeem
- Waypoints vormen het circuit
- Player en AI volgen de volgende waypoint
- Checkpoints worden geregistreerd per racer
- Lap wordt voltooid bij passage van finish trigger na checkpoint
- Ranking en finish status worden bijgehouden

### 7.5 Raceflow
- Countdown voorafgaand aan de race
- Race timer wordt gestart na countdown
- Score / positie / lap-info weergeven in HUD
- Voor de speler wordt total time, lap time en positie getoond

## 8. Spelmodi

### 8.1 Hoofdmodus: Race
- Voor één speler tegen AI
- Keuze tussen meerdere circuits
- Race op tijd of ultiem resultaat

### 8.2 Optionele variant: Time Trial / Practice
Niet officieel in de huidige code, maar mogelijk uitbreiding als vervolgproject.

## 9. Baanconcepten

### 9.1 Zandvoort
- Strakke, snelle racebaan met ritmische bochten
- Geschikt als arcade-racecircuit met duidelijke line

### 9.2 Nürburgring
- Langere, uitdagendere route met meer variatie in snelheid en bochten
- Geschikt voor meer geavanceerde rijstijl

## 10. UI en HUD
- Hoofdmenu
- Track-selectie
- Instellingen
- Pause-menu
- Countdown overlay
- Speedometer
- Lap-counter
- Timer
- Position / finish info

### 10.1 UIsystem
Het project bevat een centrale UI-controller die:
- volumeregeling toepast
- camera-FoV past aan
- AI in-/uitschakelt
- menu's activeert en pauzeert
- track instances laadt en verwijdert

## 11. Audio en sfeer
- Startgeluid
- Motor-geluid in idle en rev-mode
- Radio/ muziek in de auto
- Volume-instellingen per categorie: master, engine, music

## 12. Visuele stijl
- Realistische 3D-cars in een raceomgeving
- Gebruik van Unity 3D physics en wheel visuals
- Voornaamste focus ligt op functionaliteit en race flow, niet op ultra-realistische graphics

## 13. Tactische diepgang
Het spel is gericht op:
- consistente snelheid
- goed tijdmanagement in bochten
- remzones respecteren
- route volgen zonder te verspringen
- race tempo opbouwen en behouden

## 14. Balansprincipes
- Niet te veel vertraging door AI-fouten
- Voertuig moet goed bestuurbaar blijven
- Race moet blijven toegankelijk voor nieuwe spelers
- AI mag uitdaging bieden zonder frustratie op te wekken

## 15. Huidige projectstatus
Het project zit in een prototype/vroege ontwikkelingsfase. De kernfunctionaliteit is aanwezig:
- besturing
- AI racer
- waypoint tracking
- lap/timer systeem
- menu + settings
- circuit selectie
- finish flow

## 16. Ontwikkelingsdoelen
### Korte termijn
- Stabiliteit van AI-races
- Vaste checkpoint-logica
- Meer consistente reset- en respawn gedrag
- Verbeteren van UI-flow en finish states

### Middellange termijn
- Meer circuits
- Meer race-opties
- Verbeterde AI-vaardigheid
- Aanpassing van voertuigbalans

### Lange termijn
- Multiplayer / local split-screen
- Car selection
- Progression / unlocks
- Race events / championships

## 17. Risico's en aandachtspunten
- Waypoint- en checkpoint-logica moet consistent blijven
- AI kan te veel of te weinig remmen afhankelijk van snelheid
- Resetmechanisme kan spelers frustreren bij spelsituaties zonder route
- Race timing en finish state moeten nauwkeurig werken om fair play te garanderen

## 18. Conclusie
RaceGame is een compacte arcade-racegame in Unity met een duidelijke structuur: menu, track selectie, countdown, voertuigbesturing, AI-racers, lapcontrole en finish flow. Het project heeft al een solide basis voor een race prototype en is geschikt als uitgangspunt voor verdere uitbreiding naar een complete racer.

## 19. Verwijzingen naar projectcode
Belangrijkste componenten in het project:
- PlayerControls.cs (player.cs)
- CarController.cs (carcontroller.cs)
- EnemyController.cs (enemy.cs)
- LapController.cs
- UI.cs
- WaypointContainer.cs
- CheckPoint.cs
- BrakingPoints.cs