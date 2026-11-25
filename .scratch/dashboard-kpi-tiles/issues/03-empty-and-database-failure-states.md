# 03: Empty and database-failure states

**What to build:** The Dashboard handles its two non-normal states.
- **No Daily Results:** the six tiles still display, showing $0.00, $0.00, 0, 0, 0 and "N/A (spend is 0)".
  - A short hint appears above them: "No Daily Results yet. Import a CSV to see your totals.", with a link to the Import CSV page.
  - The hint is shown only when there are no Daily Results.
- **Database can't be reached:** the page shows "The dashboard couldn't load because of a database error. Try again later." **instead of** the tiles. It never shows zeros in that case, and the error is logged.

**Blocked by:** 01: Tracer bullet: Dashboard page with the five total tiles; 02: Overall ROI tile

**Status:** ready-for-agent

**Spec:** `.scratch/dashboard-kpi-tiles/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #9)

- [ ] If the results table is empty, spend and revenue are displayed as $0.00, and leads, impressions, and clicks as 0. ROI displays as "N/A (spend is 0)".

## Acceptance criteria

- [ ] **Built test-first** (`tdd` skill).
- [ ] **Empty data:** `KpiTotals` shows whether there are any Daily Results. The view shows the hint and its Import CSV link only when there are none.
- [ ] **Database failure:** `DashboardService` catches a `MySqlException`, logs it, and returns a `DashboardResult` holding an error message and no totals. This follows the same pattern as `ImportSummary.Failure`.
  - The view shows the message in place of the tiles.
  - It never shows zero tiles.
- [ ] **Empty-tables database test** (in `campaign_analytics_test`, tables emptied):
  - Expected totals: all 0, with ROI `null`.
  - Expected strings: $0.00, $0.00, 0, 0, 0 and "N/A (spend is 0)".
  - The result shows there are no Daily Results.
- [ ] **Database-failure test:** build the service with a connection string to a port where nothing listens (e.g. port 1, 2-second timeout).
  - Expected: an error result with the message and no totals.
  - This test touches no database. It stays in the "Test database" collection, so it is recorded in the dashboard report.
- [ ] **Test run:** the full test suite passes.
  - The dashboard verification report records the new checks.
  - The CSV import report is still written to `.scratch/csv-import/`.
- [ ] **Permissions:** no installs and no changes to `campaign_analytics`. If either becomes necessary, stop and ask the developer first. The empty state is checked in the real app only in ticket 04.
