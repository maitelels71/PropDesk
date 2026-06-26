# PropDesk – Requirements

## Business Rules: MyFundedFutures

### Consistency Rule (50%)
- No single trading day can represent more than 50% of total net profit
- Formula: Largest_Day / Net_Profit <= 0.50
- Consistency % = (Largest_Day / Net_Profit) * 100
- Eligible when Consistency % < 50%

### Example
| Field | Value |
|-------|-------|
| Largest Winning Day | $526 |
| Net Profit | $857 |
| Consistency % | 61.38% → NOT eligible |
| After more trading → Net Profit | $1,057 |
| Consistency % | 49.76% → ELIGIBLE ✅ |

### Buffer Rule
- Must maintain a minimum buffer above drawdown threshold
- Buffer = Current Balance - Maximum Drawdown Level

### Payout Rules
- Minimum trading days required
- Minimum profit target
- Consistency must be < 50%
- Buffer must be maintained

### Daily Safe Profit Formula
Max safe profit today = (Net_Profit_After / 0.50) - Largest_Day
where Net_Profit_After = Net_Profit + Today_Profit