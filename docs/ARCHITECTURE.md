# PropDesk – Architecture

## Solution Structure

```
PropDesk/
├── docs/                    # Documentation
├── excel/                   # Phase 1: Excel Dashboard
│   └── PropDesk.xlsm
├── src/
│   ├── PropDesk.Domain/     # Models, Entities
│   ├── PropDesk.Core/       # Business Rules Engine
│   ├── PropDesk.Infrastructure/  # SQLite, CSV Import
│   ├── PropDesk.UI/         # WPF Application
│   └── PropDesk.Reporting/  # PDF, Excel Export
└── tests/
    └── PropDesk.Tests/      # Unit Tests
```

## Business Rule Engine
```
Input (Trades/Days)
     ↓
ConsistencyEngine
     ↓
DrawdownEngine
     ↓
BufferEngine
     ↓
PayoutEngine
     ↓
EligibilityResult → Dashboard
```

## Database (SQLite)
- Accounts
- Trades
- TradingDays
- Statistics
- PayoutCycles
- PropFirms
- Settings