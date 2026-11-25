# Verification report: manual dashboard KPI tile check

- Date: 2026-11-24
- Database: `campaign_analytics` (the real database; only the developer touched it)
- Checked by: the developer, in the running app and with `.scratch/dashboard-kpi-tiles/manual-kpi-queries.sql`
- Ticket: `.scratch/dashboard-kpi-tiles/issues/04-final-manual-check-and-notes.md`

Filled in by the agent from the figures the developer gave.

## Step 2: read-only row count

| Check | Expected (clean file only) | Actual | Result |
|---|---|---|---|
| `SELECT COUNT(*) FROM results` | 240 | not taken | not checked |

Outcome: the row count was not taken before the tables were emptied. The tables were emptied on the expectation that the error file's Daily Results from PBI 4's final run were still loaded. The clean file was then re-imported (steps 3–5 run), so the check below starts from known data either way.

## Step 3: emptying `results` and `campaigns`

- Developer approval given at the time: yes (the developer emptied the tables themselves)
- Tables emptied: `results`, `campaigns` (Channels not touched): yes

## Step 4: empty state, checked by eye on `/Dashboard`

| Check | Expected | Seen | Result |
|---|---|---|---|
| Hint | No Daily Results yet. Import a CSV to see your totals. | No Daily Results yet. Import a CSV to see your totals. | match |
| Import CSV link in the hint opens the Import CSV page | yes | yes | match |
| Total Spend tile | $0.00 | $0.00 | match |
| Total Revenue tile | $0.00 | $0.00 | match |
| Total Leads tile | 0 | 0 | match |
| Total Impressions tile | 0 | 0 | match |
| Total Clicks tile | 0 | 0 | match |
| Overall ROI tile | N/A (spend is 0) | N/A (spend is 0) | match |

## Step 5: re-import of `sample-data/campaign_results_clean.csv`

| Check | Expected | Actual | Result |
|---|---|---|---|
| Imported | 240 | 240 | match |
| Duplicates | 0 | 0 | match |
| Rejected | 0 | 0 | match |

## Step 6: tiles against the manual queries

Dashboard opened from the navigation bar's Dashboard link: yes

| Tile | Expected tile | Tile shows | Query result | Result |
|---|---|---|---|---|
| Total Spend | $45,256.13 | $45,256.13 | 45256.13 | match |
| Total Revenue | $82,078.31 | $82,078.31 | 82078.31 | match |
| Total Leads | 1,630 | 1,630 | 1630 | match |
| Total Impressions | 4,995,183 | 4,995,183 | 4995183 | match |
| Total Clicks | 37,882 | 37,882 | 37882 | match |
| Overall ROI | 81.4% | 81.4% | 81.4 | match |

Every tile matches its expected value and its manual query.

## Not checked here

The "total spend is 0" and "Overall ROI uses totals, not an average" cases are covered only by the automated tests in `verification-report-tests.md`. No made-up rows went into the real database.
