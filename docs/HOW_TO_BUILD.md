# PropDesk – How to Build & Run

## Prerequisites
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (Community is free) OR VS Code with C# extension

## Quick Start (Command Line)

```bash
# 1. Clone the repo
git clone https://github.com/maitelels71/PropDesk.git
cd PropDesk

# 2. Build everything
cd src
dotnet restore
dotnet build

# 3. Run the app
dotnet run --project PropDesk.UI

# 4. Run unit tests
cd ../tests/PropDesk.Tests
dotnet test
```

## Visual Studio 2022
1. Open `src/PropDesk.sln`
2. Set `PropDesk.UI` as Startup Project
3. Press F5 to run

## Project Structure
```
PropDesk/
├── docs/               ← Documentation
├── excel/              ← Phase 1: Excel Dashboard (use immediately)
│   └── PropDesk_Dashboard.xlsx
└── src/
    ├── PropDesk.Domain/          ← Models: Trade, Account, PayoutCycle
    ├── PropDesk.Core/            ← Business engines (Consistency, Drawdown, Payout)
    ├── PropDesk.Infrastructure/  ← SQLite database + CSV importer
    ├── PropDesk.UI/              ← WPF desktop application
    └── PropDesk.Reporting/       ← HTML/PDF report generator
```

## Using the App

### First Launch
- App opens with demo data (Net=$857, Largest Day=$526 – the exact spec example)
- The consistency shows 61.38% → NOT eligible (correct per spec)
- Go to ⚙ Settings to configure your real account

### Import Your Trades
1. Export trades CSV from MyFundedFutures platform
2. Click **📂 Import CSV** in the app
3. Select your CSV file → preview appears
4. Click **✅ Import Trades**
5. Dashboard updates automatically

### Using the Excel Dashboard (Phase 1)
- Open `excel/PropDesk_Dashboard.xlsx`
- Go to **📅 Calendar** sheet
- Enter your date and daily P&L for each trading day
- Everything else updates automatically:
  - Consistency %, Eligibility, Buffer on Dashboard
  - Charts on 📈 Charts sheet

## NuGet Packages (auto-restored)
| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.3.2 | MVVM framework |
| LiveChartsCore.SkiaSharpView.WPF | 2.0.0-rc3 | Charts |
| Microsoft.Data.Sqlite | 8.0.0 | Database |
| MaterialDesignThemes | 5.1.0 | UI components |

## Database
- SQLite file created automatically at `propdesk.db` (next to the .exe)
- No setup required
- Backup with **🗄 Backup** button (coming Sprint 5)
