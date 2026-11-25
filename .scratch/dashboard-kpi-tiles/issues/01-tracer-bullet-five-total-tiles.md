# 01: Tracer bullet: Dashboard page with the five total tiles

**What to build:** A marketer clicks "Dashboard" in the navigation and sees five KPI tiles: Total Spend, Total Revenue, Total Leads, Total Impressions and Total Clicks.
- **Totals:** each one is summed over every Daily Result in `results` by one SQL query.
- **Money:** shown in dollars with commas and two decimals, e.g. $45,256.13.
- **Counts:** shown with commas, e.g. 4,995,183.

Before the page is built, `VerificationReport` is changed to write one report per feature folder, and the full test suite is run to prove the CSV import tests and their report are unaffected.

This slice leaves out the Overall ROI tile (ticket 02), the empty-state hint and the database-failure message (ticket 03).

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

**Spec:** `.scratch/dashboard-kpi-tiles/spec.md`

## Order of work

1. **Prefactor: the verification report.** Do this **before building the page**.
   - Tag each test with its feature folder, and write one `verification-report-tests.md` per folder.
   - The CSV import tests keep writing to `.scratch/csv-import/`.
   - All tests stay in the single "Test database" collection.
   - Run the **full** test suite. Every existing CSV import test passes, and `.scratch/csv-import/verification-report-tests.md` is rewritten with a new run time and the same checks.
2. **Build the page, test-first** (`tdd` skill).
3. **Run the full test suite again.**
   - All tests pass.
   - The CSV import report is still written to `.scratch/csv-import/`.
   - `.scratch/dashboard-kpi-tiles/verification-report-tests.md` is created and holds only the dashboard tests.

## PBI acceptance criteria delivered (verbatim from issue #9)

- [ ] The dashboard link in the navigation bar opens the dashboard page.
- [ ] With the clean sample CSV file imported, the spend tile displays $45,256.13, the revenue tile displays $82,078.31, the leads tile displays 1,630, the impressions tile displays 4,995,183, and the clicks tile displays 37,882.

## Acceptance criteria

- [ ] **Prefactor:** the `VerificationReport` change is made first, and the full suite is run before any dashboard code is written. All CSV import tests pass, and their report is still written to `.scratch/csv-import/`.
- [ ] **Navigation:** a "Dashboard" link sits between Home and Import CSV and opens `/Dashboard` (`DashboardController.Index`). The Home page is unchanged.
- [ ] **Service:** `DashboardService.GetKpiTotalsAsync()` runs **one** SQL query on `results` that sums spend, revenue, leads, impressions and clicks, each wrapped in COALESCE.
  - It returns a `DashboardResult` holding `KpiTotals`.
  - Money totals are `decimal`. Count totals are `long`.
- [ ] **Configuration:** the connection string comes from configuration (User Secrets) through dependency injection, the same way as `CsvImportService`.
- [ ] **Formatter:** `KpiFormatter` is a pure static class using the invariant culture with explicit patterns.
  - Money is "$" followed by the value with commas and 2 decimals.
  - Counts have commas and no decimals.
  - It does not use culture-dependent currency formatting.
- [ ] **Tiles:** five Bootstrap cards appear in the order Total Spend, Total Revenue, Total Leads, Total Impressions, Total Clicks.
  - Each shows its label in small text above its value in large text.
  - Colours are neutral.
  - The grid has room for the sixth tile in a 3 / 2 / 1 layout.
  - The view only displays strings from `KpiFormatter`.
- [ ] **Clean-file database test** (in `campaign_analytics_test`, tables emptied first):
  - Import the clean sample CSV through `CsvImportService`.
  - Expected totals: 45,256.13 / 82,078.31 / 1,630 / 4,995,183 / 37,882.
  - Expected strings: $45,256.13, $82,078.31, 1,630, 4,995,183, 37,882.
- [ ] **Formatter tests:**

  | Input | Expected text |
  |---|---|
  | money 1234567.5 | $1,234,567.50 |
  | money 0 | $0.00 |
  | count 0 | 0 |
  | count 4995183 | 4,995,183 |
- [ ] **Final test run:** the full suite is run after the page is built.
  - All tests pass.
  - Both reports are written to their own folders, each with actual figures next to the expected ones.
- [ ] **Manual check (developer):** with data loaded in `campaign_analytics`, the Dashboard link opens the page and the five tiles display.
- [ ] **Permissions:** no installs and no changes to `campaign_analytics`. If either becomes necessary, stop and ask the developer first.
