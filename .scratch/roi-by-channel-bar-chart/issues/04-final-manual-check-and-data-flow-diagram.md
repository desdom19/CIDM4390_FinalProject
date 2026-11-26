# 04: Final manual check against campaign_analytics, and data flow diagram

**What to build:** An end-to-end demonstration of the PBI's acceptance criteria in the real app against the real `campaign_analytics` database.
- **Empty state:** the developer sees it by eye.
- **Clean data:** after re-importing the clean sample CSV, the developer checks every bar against their own SQL queries.
- **Records:** the actual figures go into a manual verification report, with screenshots.
- **Diagram:** the developer then writes `roi-by-channel-data-flow.md`.

The agent's only parts are preparing the queries file and the report template, and filling in the report from the figures the developer gives. **The agent never writes `roi-by-channel-data-flow.md`.**

Only the developer touches `campaign_analytics`. Emptying its `results` and `campaigns` tables is a database change that needs the developer's separate, explicit approval at the time. Channels are never touched.

**Blocked by:** 01: Tracer bullet: ROI by Channel bars for the clean CSV; 02: Sorting, mixed scaling and negative bars; 03: Left-out Channels, the empty state and the database error

**Status:** ready-for-human

**Spec:** `.scratch/roi-by-channel-bar-chart/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #11)

- [ ] A bar chart is displayed under the KPI tiles in the dashboard page.
- [ ] With the clean sample CSV imported, each bar percentage displays as the following:
email: 155.1%
social: 24.8%
search: 131.4%
display: 46.4%
- [ ] If the results table is empty, the chart area shows no bars and a short message and link to Import CSV page.

Developer's own task (the knowledge artifact):

- [ ] roi-by-channel-data-flow.md is committed to the repository, and displays a data flow diagram that shows how the data flows from results through campaigns and channels to the bar chart.

The no-spend, negative and totals-not-average cases are checked only by the automated tests in 01–03. No made-up rows go into the real database.

## Steps

1. **Agent prepares** `.scratch/roi-by-channel-bar-chart/manual-roi-queries.sql`:
   - a read-only row count of `results`
   - four separate SELECTs, one per Channel, each filtered by Channel name through `campaigns` and `channels` (deliberately not the app's grouped query). Each returns the Channel's total spend, total revenue and ROUND((revenue − spend) ÷ spend × 100, 1).

   The agent also prepares the template `.scratch/roi-by-channel-bar-chart/verification-report-manual.md`.
2. **Developer runs the read-only row count** and records it. Seeing the empty chart for real means emptying the tables, so step 3 follows either way.
3. **Developer approves emptying the tables, then empties them.** This is a separate, explicit approval, given at the time. Only `results` and `campaigns` are emptied. If the developer chooses not to empty them, record that the empty state was checked only by the automated test, and skip to step 5.
4. **Developer opens `/Dashboard` and checks the empty chart by eye:**
   - no bars
   - "No Daily Results yet. Import a CSV to see ROI by Channel." with its Import CSV link, which opens the Import CSV page

   The developer screenshots it into `.scratch/roi-by-channel-bar-chart/images/`.
5. **Developer re-imports `sample-data/campaign_results_clean.csv`** through the Import CSV page. Expected: 240 imported, 0 duplicates, 0 rejected.
6. **Developer opens `/Dashboard` and runs the manual queries.**
   - The chart sits under the KPI tiles.
   - Each bar's label is compared with its query result.
   - The order from top to bottom is Email, Search, Display, Social.

   The developer screenshots it into `images/`.
7. **Agent fills in `verification-report-manual.md`** from the figures the developer gives: the row count, the empty-state observation, the import summary, and for each Channel the bar's label, the query result and the expected value.
8. **Developer writes `roi-by-channel-data-flow.md`.** The developer's task only.

## Acceptance criteria

- [ ] Developer approval is given before `results` and `campaigns` in `campaign_analytics` are emptied (if they are). Channels are not touched.
- [ ] If the tables were emptied: the empty chart is checked by eye and screenshotted before the re-import, and the result is recorded.
- [ ] With the clean file loaded, the chart under the tiles shows Email 155.1%, Search 131.4%, Display 46.4%, Social 24.8%, and is screenshotted.
- [ ] Each of those labels matches its manual SQL query.
- [ ] The actual figures are recorded in `verification-report-manual.md`.
- [ ] After the manual check, the developer writes and commits `roi-by-channel-data-flow.md`.
