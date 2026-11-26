# 01: Tracer bullet: ROI by Channel bars for the clean CSV

**What to build:** A marketer opens the Dashboard and, under the KPI tiles, sees a chart headed "ROI by Channel".
- **Bars:** one horizontal bar per Channel with spend, all in one neutral colour.
- **Names and labels:** each Channel name is capitalised (Email, Social, Search, Display), and each bar is labelled with its ROI from `KpiFormatter.Roi`, placed just past the bar's end.
- **ROI:** each Channel's ROI is calculated from that Channel's total revenue and total spend.
- **Same snapshot:** the Channel figures are read in the same snapshot as the tiles.
- **Bar lengths:** worked out by the `RoiChart` builder, with the zero line at the left edge.

This slice draws the bars in the order the query returns them. It leaves out:
- sorting, the mixed positive/negative scale and left-growing negative bars (ticket 02)
- left-out Channels, the empty and no-spend messages, and the database error for the chart (ticket 03)

**Blocked by:** None (can start immediately)

**Status:** done

**Spec:** `.scratch/roi-by-channel-bar-chart/spec.md`

## Order of work

1. **Prefactor: the service method and the report.** Do this **before building the chart**.
   - Replace `DashboardService.GetKpiTotalsAsync()` with `GetDashboardAsync()`. The existing totals query runs inside one read-only `REPEATABLE READ` transaction on one connection.
   - Move the existing PBI 5 tests over to the new method. Their checks don't change.
   - Update the KPI tiles `ReportFeature`'s seam text to the new method name. Add a `ReportFeature` for `roi-by-channel-bar-chart`.
   - Run the **full** test suite. Every existing test passes, and both `.scratch/csv-import/verification-report-tests.md` and `.scratch/dashboard-kpi-tiles/verification-report-tests.md` are rewritten with a new run time and the same checks.
2. **Build the chart, test-first** (`tdd` skill).
3. **Run the full test suite again.**
   - All tests pass.
   - The two existing reports are still written to their own folders.
   - `.scratch/roi-by-channel-bar-chart/verification-report-tests.md` is created and holds only this PBI's tests.

## PBI acceptance criteria delivered (verbatim from issue #11)

- [ ] A bar chart is displayed under the KPI tiles in the dashboard page.
- [ ] With the clean sample CSV imported, each bar percentage displays as the following:
email: 155.1%
social: 24.8%
search: 131.4%
display: 46.4%

## Acceptance criteria

- [ ] **Prefactor:** the method rename and the report changes are made first, and the full suite is run before any chart code is written. All existing tests pass, and both existing reports are rewritten in their own folders.
- [ ] **Snapshot:** `GetDashboardAsync()` runs the totals query and a new per-Channel query on one connection, in one read-only `REPEATABLE READ` transaction. It returns a `DashboardResult` holding `KpiTotals` and the Channel totals.
- [ ] **Per-Channel query:** it starts from `channels` and LEFT JOINs `campaigns` and `results`, grouped by Channel. Spend and revenue are summed with COALESCE, so all four Channels always come back.
- [ ] **`ChannelTotals`:** a Channel's stored name, total spend and total revenue (`decimal`). Its ROI is (revenue − spend) ÷ spend as a `decimal?`, `null` when total spend is exactly 0.
- [ ] **`RoiChart`:** a pure static builder that returns the bars the view draws. Each bar has:
  - the display name: the stored name with its first letter capitalised, invariant culture
  - the label from `KpiFormatter.Roi`
  - a width, as a percentage of the track: the ROI ÷ the largest ROI × 100

  The zero line is at 0. An ROI of exactly 0 has width 0 and draws no bar. There is no minimum width.
- [ ] **View:**
  - A section headed "ROI by Channel" sits under the tiles. Each row shows the capitalised name, a track, a neutral Bootstrap bar and its label just past the bar's end, with room in the track so labels aren't clipped.
  - Each row reads as text ("Email 155.1%"), and the bar is `aria-hidden`.
  - Widths go into inline styles using the invariant culture.
  - The view only loops over `RoiChart`'s result.
- [ ] **Clean-file database test** (in `campaign_analytics_test`, tables emptied first), importing the clean sample CSV through `CsvImportService`:
  - Expected Channel totals:

    | Channel | Spend | Revenue | ROI (to 4 decimals) | Label |
    |---|---|---|---|---|
    | email | 2,875.19 | 7,333.83 | 1.5507 | 155.1% |
    | social | 13,750.29 | 17,155.23 | 0.2476 | 24.8% |
    | search | 18,451.80 | 42,691.58 | 1.3137 | 131.4% |
    | display | 10,178.85 | 14,897.67 | 0.4636 | 46.4% |

  - The four Channel spends add up to Total Spend, 45,256.13.
- [ ] **Totals-not-average database test:** two Campaigns on the same Channel, one spending $100 and earning $300 and another spending $10,000 and earning $5,000. Expected: that Channel's label is -47.5%, not the +75% average. Ticket 02 adds the check that it is drawn as a negative bar.
- [ ] **`RoiChart` unit tests:**
  - widths are proportional to the ROI, with the largest at 100
  - the zero line is at 0
  - an ROI of exactly 0 gives width 0
  - display names are capitalised
  - widths are the same whatever the current culture
- [ ] **Final test run:** the full suite is run after the chart is built.
  - All tests pass.
  - All three reports are written to their own folders, each with actual figures next to the expected ones.
- [ ] **Manual check (developer):** with the clean data loaded in `campaign_analytics`, the chart appears under the tiles with four labelled bars.
- [ ] **Permissions:** no installs and no changes to `campaign_analytics`. If either becomes necessary, stop and ask the developer first.
