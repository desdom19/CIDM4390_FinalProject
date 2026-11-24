# 04: Final manual check against campaign_analytics, and notes

**What to build:** An end-to-end demonstration of the PBI's acceptance criteria in the real app against the real `campaign_analytics` database.
- **Empty state:** the developer sees it by eye.
- **Clean data:** after re-importing the clean sample CSV, the developer checks every tile against their own SQL queries.
- **Records:** the actual figures are recorded in a manual verification report.
- **Notes:** the developer then writes `dashboard-tiles-creation-notes.md`.

The agent's only parts are preparing the queries file and the report template, and filling in the report from the figures the developer gives. **The agent never writes `dashboard-tiles-creation-notes.md`.**

Only the developer touches `campaign_analytics`. Emptying its `results` and `campaigns` tables is a database change that needs the developer's separate, explicit approval at the time. Channels are never touched.

**Blocked by:** 01: Tracer bullet: Dashboard page with the five total tiles; 02: Overall ROI tile; 03: Empty and database-failure states

**Status:** ready-for-human

**Spec:** `.scratch/dashboard-kpi-tiles/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #9)

- [ ] The dashboard link in the navigation bar opens the dashboard page.
- [ ] With the clean sample CSV file imported, the spend tile displays $45,256.13, the revenue tile displays $82,078.31, the leads tile displays 1,630, the impressions tile displays 4,995,183, and the clicks tile displays 37,882.
- [ ] The ROI tile displays 81.4% as calculated from (revenue - spend) / spend.
- [ ] If the results table is empty, spend and revenue are displayed as $0.00, and leads, impressions, and clicks as 0. ROI displays as "N/A (spend is 0)".
- [ ] dashboard-tiles-creation-notes.md is committed to the repository root and states how each KPI tile is derived, formatted, and verified manually.

The "total spend is 0" criterion is checked only by the automated test in 02. No made-up rows go into the real database.

## Steps

1. **Agent prepares** `.scratch/dashboard-kpi-tiles/manual-kpi-queries.sql`:
   - one separate SELECT per tile (spend, revenue, leads, impressions, clicks), deliberately not the app's combined query
   - an ROI query rounded to one decimal, (revenue − spend) ÷ spend × 100
   - a read-only row count of `results`

   The agent also prepares the template `.scratch/dashboard-kpi-tiles/verification-report-manual.md`.
2. **Developer runs the read-only row count.** If `results` doesn't hold exactly the clean file's 240 Daily Results, go on to step 3. Otherwise skip to step 5 and record that the clean data was already loaded.
3. **Developer approves emptying the tables, then empties them.** This is a separate, explicit approval, given at the time. Only `results` and `campaigns` are emptied.
4. **Developer opens `/Dashboard` and checks the empty state by eye:**
   - the hint "No Daily Results yet. Import a CSV to see your totals." with its Import CSV link
   - $0.00, $0.00, 0, 0, 0 and "N/A (spend is 0)"
5. **Developer re-imports `sample-data/campaign_results_clean.csv`** through the Import CSV page. Expected: 240 imported, 0 duplicates, 0 rejected.
6. **Developer opens `/Dashboard` from the nav link and runs the manual queries.** Each tile's value is compared with its query result.
7. **Agent fills in `verification-report-manual.md`** from the figures the developer gives: the row count, the empty-state observation, the import summary, and for each tile the tile's text, the query result and the expected value.
8. **Developer writes `dashboard-tiles-creation-notes.md`.** The developer's task only.

## Acceptance criteria

- [ ] Developer approval is given before `results` and `campaigns` in `campaign_analytics` are emptied (if they need to be). Channels are not touched.
- [ ] If the tables were emptied: the empty dashboard is checked by eye before the re-import, and the result is recorded.
- [ ] With the clean file loaded, the dashboard shows $45,256.13, $82,078.31, 1,630, 4,995,183, 37,882 and 81.4%.
- [ ] Each of those values matches its manual SQL query.
- [ ] The actual figures are recorded in `verification-report-manual.md`.
- [ ] After the manual check, the developer writes and commits `dashboard-tiles-creation-notes.md`.
