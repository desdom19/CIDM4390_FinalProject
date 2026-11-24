# 02: Overall ROI tile

**What to build:** The Dashboard gets its sixth tile, Overall ROI, placed after Total Clicks.
- **Calculation:** (total revenue − total spend) ÷ total spend, from the same totals the spend and revenue tiles show. It is never an average of each Campaign's ROI (see ROI in the glossary).
- **Display:** a percentage rounded to one decimal, e.g. 81.4%.
  - A loss shows a minus sign, e.g. -12.3%.
  - A value that rounds to zero always shows 0.0%, never -0.0%.
  - When total spend is exactly 0, the tile shows "N/A (spend is 0)". This holds even when there is revenue, and the revenue tile still shows its real total.

**Blocked by:** 01: Tracer bullet: Dashboard page with the five total tiles

**Status:** ready-for-agent

**Spec:** `.scratch/dashboard-kpi-tiles/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #9)

- [ ] The ROI tile displays 81.4% as calculated from (revenue - spend) / spend.
- [ ] If total spend is 0, the ROI tile displays "N/A (spend is 0)".

## Acceptance criteria

- [ ] **Built test-first** (`tdd` skill).
- [ ] **Calculation:** `KpiTotals` exposes Overall ROI as a `decimal?` calculated in C# from total revenue and total spend. It is `null` when total spend is exactly 0. There is no extra SQL query.
- [ ] **Formatting:** `KpiFormatter` formats ROI using the invariant culture with explicit patterns.
  - The value × 100 with commas and 1 decimal, followed by "%".
  - Midpoints round away from zero.
  - A result that rounds to zero is shown as "0.0%".
  - `null` is shown as "N/A (spend is 0)".
  - It does not use invariant percent formatting, which would put a space before the "%".
- [ ] **Tile:** an Overall ROI tile with neutral colours follows Total Clicks, completing the 3 / 2 / 1 grid.
- [ ] **Clean-file database test:** extend the test from 01 to also expect ROI ≈ 0.8136 and the string "81.4%".
- [ ] **Zero-spend database test:** a small inline CSV where every row has spend 0 and some revenue.
  - Expected: ROI `null`, the string "N/A (spend is 0)", and the real revenue total.
- [ ] **Totals-not-average database test:** a small inline CSV with one Campaign spending $100 and earning $300, and another spending $10,000 and earning $5,000.
  - Expected: Overall ROI shows "-47.5%", not the +75% average of the two ROIs.
- [ ] **Formatter tests:**

  | Input | Expected text |
  |---|---|
  | 0.813640 | 81.4% |
  | 0.81350 (a midpoint) | 81.4% |
  | −0.12345 | -12.3% |
  | −0.0004 | 0.0% |
  | `null` | N/A (spend is 0) |
- [ ] **Test run:** the full test suite passes, and the dashboard verification report records the new checks with actual figures next to the expected ones.
- [ ] **Manual check (developer):** with data loaded in `campaign_analytics`, the Overall ROI tile displays.
- [ ] **Permissions:** no installs and no changes to `campaign_analytics`. If either becomes necessary, stop and ask the developer first.
