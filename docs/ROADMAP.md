# PropDesk – Roadmap

## ✅ Sprint 1 – Foundation
- [x] Architecture & domain models (Trade, TradingDay, Account, PayoutCycle)
- [x] ConsistencyEngine – 50% rule calculator
- [x] DrawdownEngine, PayoutEngine, StatisticsEngine
- [x] CSV Importer for MyFundedFutures
- [x] Unit tests (ConsistencyEngine, PayoutEngine)

## ✅ Sprint 2 – Excel Dashboard (Phase 1)
- [x] Dashboard sheet – all metrics auto-calculated
- [x] Calendar sheet – type daily P&L, rest updates automatically
- [x] Consistency Calculator interactive sheet
- [x] Payout History sheet
- [x] Charts sheet – Equity Curve, Daily P&L, Consistency Trend

## ✅ Sprint 3 – WPF Core (Phase 3)
- [x] MainWindow dark theme MVVM layout
- [x] SettingsWindow – configure all prop firm rules
- [x] ImportWindow – CSV preview grid + import to SQLite
- [x] SQLite schema + AccountRepository + TradeRepository + PayoutCycleRepository
- [x] MainViewModel fully wired to database

## ✅ Sprint 4 – Charts & Reporting
- [x] LiveCharts2 EquityChartView (3 chart types: equity, daily P&L, consistency)
- [x] ChartViewModel with tab switcher + mini stats
- [x] PayoutHistoryWindow with summary cards + sortable table
- [x] PdfReportService – HTML report generation
- [x] Export Report button – opens in browser
- [x] Two-panel MainWindow layout (metrics left, chart right)

## 🔜 Sprint 5 – Polish & Multi-Account
- [ ] Multi-account switcher in header
- [ ] Backup / Restore database
- [ ] ClosedXML Excel export of report
- [ ] iText7 true PDF export
- [ ] App icon + splash screen

## 🔜 Sprint 6 – Release
- [ ] Installer (.exe via Inno Setup or MSIX)
- [ ] Multiple prop firms (Apex, TopStep rules)
- [ ] User documentation PDF
- [ ] Auto-update check