# ScoreCounter (Poängräknaren)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-5C2D91?logo=dotnet&logoColor=white)
![Blazor WebAssembly](https://img.shields.io/badge/Blazor%20WebAssembly-512BD4?logo=blazor&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?logo=microsoftazure&logoColor=white)
![Azure SQL](https://img.shields.io/badge/Azure%20SQL-CC2927?logo=microsoftsqlserver&logoColor=white)
![Azure Key Vault](https://img.shields.io/badge/Azure%20Key%20Vault-0078D4?logo=microsoftazure&logoColor=white)
![Azure SQL Database](https://img.shields.io/badge/Azure%20SQL%20Database-CC2927?logo=microsoftsqlserver&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-68217A?logo=dotnet&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

<p align="center">
  <img src="Images/CreateMatch.png" width="300"/>
</p>

<p align="center">
  <img src="Images/Match.png" width="300"/>
</p>

<p align="center">
  <img src="Images/GoToMatch.png" width="300"/>
</p>

<p align="center">
  <img src="Images/Final Results.png" width="300"/>
</p>

## Beskrivning

ScoreCounter är en realtidsapplikation för att skapa och hantera matcher med spelare och poäng.

Systemet låter användare skapa matcher, lägga till spelare och uppdatera poäng i realtid med hjälp av SignalR.
Alla ändringar uppdateras direkt i alla anslutna klienter.

Systemet låter användare:

- Skapa matcher med valfritt spelnamn, startpoäng, målpoäng och max spelare
- Välja "högst poäng vinner" eller "lägst poäng vinner"
- Lägga till och ta bort spelare (med bekräftelsedialog)
- Uppdatera poäng i realtid med knappar (-1, -5, +1, +5) eller anpassad poäng
- Se poänghistorik per spelare med tid, ändring och poäng
- Klona eller återställa matcher
- Avsluta matcher och visa resultat med vinnare eller oavgjort (draw)
- Dela matcher via QR-kod
- Följa uppdateringar direkt via SignalR på alla enheter

---

## Funktioner

### Matchhantering

- Skapa match med valfritt spelnamn
- Sätta max antal spelare (med validering)
- Sätta målpoäng (Max Score) som visas på matchen
- Låsa möjligheten att lägga till fler spelare
- Avsluta match
- Klona match (behåller spelare, inställningar och poäng)
- Reset match till ursprungsläge
- Resultat-popup med vinnare, oavgjort (draw) och fullständig placeringstabell
- QR-kodsdelning för att gå direkt till matchen
- Kopiera matchlänk

### Spelare

- Lägg till spelare
- Ta bort spelare (med bekräftelsedialog)
- Ta bort spelare direkt i Create Game-vyn
- Byt namn (uppdateras även i poänghistoriken)
- Kontroll av duplicerade namn
- Max Players-validering — kan inte sänkas under antal tillagda spelare

### Poängsystem

- Öka poäng (+1, +5)
- Minska poäng (-1, -5)
- Anpassad poängändring (ange valfritt antal)
- Optimistisk UI-uppdatering — poängen visas direkt, återställs vid fel
- Realtidsuppdatering

### Poänghistorik

- Alla poängändringar sparas i databasen (ScoreEntry)
- Historik visas grupperad per spelare med tid, ändring och poäng (före → efter)
- Uppdateras i realtid via SignalR
- Historik rensas automatiskt när spelare tas bort
- Namnbyte uppdateras i historiken

### Gränssnitt

- Dark / Light mode
- Responsiv design (mobil, tablet, desktop)
- Global loading spinner vid alla HTTP-anrop
- Statusmeddelanden i färgad bar
- Score-knappar med anpassat mörkt tema

### Realtid (SignalR)

- ScoreChanged
- ScoreHistoryEntry
- PlayerAdded
- PlayerRemoved
- PlayerRenamed
- ScoreHistoryRenamed
- PlayerHistoryRemoved
- MatchFinished
- MatchReset

---

## Arkitektur

Projektet är uppdelat i tre huvuddelar:

### Backend

- ASP.NET Core Minimal API
- SignalR Hub
- Entity Framework Core
- MatchStore
- SQLite (development)
- SQL Server (production / Azure)

### Frontend

- Blazor WebAssembly
- SignalR
- Responsiv UI
- Sidebar navigation
- Realtidsuppdatering
- QR-kodsgenerering via QRCoder

### Shared

- Delade modeller (GameMatch, GamePlayer, ScoreEntry)
- DTOs (Data Transfer Objects)
- Gemensamma regler och kontrakt
- Säkerställer att backend och frontend använder samma datastruktur

---

### API Endpoints

#### Match

- `GET /api/match/{id}` → Hämta match
- `POST /api/match` → Skapa match
- `POST /api/match/{id}/reset` → Reset match
- `POST /api/match/{id}/clone` → Klona match
- `POST /api/match/{id}/finish` → Avsluta match
- `GET /api/match/{id}/history` → Hämta poänghistorik

---

#### Spelare

- `POST /api/match/{id}/player` → Lägg till spelare
- `PUT /api/match/{id}/player/{playerId}/score` → Uppdatera poäng
- `PUT /api/match/{id}/player/{playerId}/name` → Byt namn
- `DELETE /api/match/{id}/player/{playerId}` → Ta bort spelare

---

### SignalR Hub

Endpoint: /matchevents

---

## Flöde i systemet

- Skapa match
- Lägg till spelare
- Anslut klienter via SignalR
- Uppdatera poäng
- Realtidsuppdatering till alla klienter
- Poänghistorik sparas och synkas
- Avsluta eller reset match

## Så här kör du programmet

```
cd Backend
dotnet run
```

## Konfiguration

- Lokal databas (development):
  `Data Source=scorecounter.db`
- Produktion:
  - Azure SQL Server
  - Azure Key Vault (hemlighet: `SqlScoreCounterConnectionString`)

## Publicering till Azure Portal

Programmet är förberett för att hostas på Microsoft Azure. Följ dessa steg för att sätta upp allt via Azure Portal:

1. **Skapa en Azure SQL-databas**
   - Gå till Azure Portal och skapa en SQL Database och en SQL Server.
   - Spara anslutningssträngen (Connection String) för databasen.

2. **Skapa en Azure Key Vault**
   - Skapa en Key Vault i samma resource group.
   - Lägg till en hemlighet (Secret) med namnet `SqlScoreCounterConnectionString` och klistra in anslutningssträngen från din SQL-databas.

3. **Skapa en App Service för backend**
   - Skapa en App Service.
   - Publicera backend-projektet till denna App Service (t.ex. via GitHub Actions eller ZIP deploy).

4. **Aktivera Managed Identity för App Service**
   - Gå till App Service > Identity > System assigned > Sätt till "On" och spara.

5. **Ge Key Vault access till App Service**
   - Gå till Key Vault > Access control (IAM) > Lägg till rolltilldelning.
   - Välj rollen "Key Vault Secrets User" och välj din App Service som principal.

### Deploya med skript

```powershell
.\deploy.ps1
```

Skriptet bygger både Frontend och Backend, publicerar och deployar via ZIP.

---

## Tekniker

- ASP.NET Core
- Blazor WebAssembly
- SignalR
- Entity Framework Core
- SQLite / SQL Server
- Azure
- QRCoder

---

## Utvecklare

- Alaa Alsous
- Astrid Skoglund
- Andreas Fransson
- Daniel Viklund
