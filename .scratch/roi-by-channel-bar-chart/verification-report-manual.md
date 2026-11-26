# Verification report: manual ROI by Channel chart check

- Date: 2026-11-26
- Database: `campaign_analytics` (the real database; only the developer touched it)
- Checked by: the developer, in the running app and with `.scratch/roi-by-channel-bar-chart/manual-roi-queries.sql`
- Ticket: `.scratch/roi-by-channel-bar-chart/issues/04-final-manual-check-and-data-flow-diagram.md`

Filled in by the agent from the figures the developer gave.

## Step 2: read-only row count

| Check | Expected (clean file only) | Actual | Result |
|---|---|---|---|
| `SELECT COUNT(*) FROM results` | 240 | 240 | match |

## Step 3: emptying `results` and `campaigns`

- Developer approval given at the time: yes (the developer emptied the tables themselves)
- Tables emptied: `results`, `campaigns` (Channels not touched): yes

If the tables were not emptied, the empty state was checked only by the automated test in `verification-report-tests.md`, and step 4 is skipped.

## Step 4: empty chart, checked by eye on `/Dashboard`

| Check | Expected | Seen | Result |
|---|---|---|---|
| Bars in the chart area | none | none | match |
| Message | No Daily Results yet. Import a CSV to see ROI by Channel. | No Daily Results yet. Import a CSV to see ROI by Channel. | match |
| "Import a CSV" link opens the Import CSV page | yes | yes | match |

Screenshot: `images/emptyroibychannel.png`

## Step 5: re-import of `sample-data/campaign_results_clean.csv`

| Check | Expected | Actual | Result |
|---|---|---|---|
| Imported | 240 | 240 | match |
| Duplicates | 0 | 0 | match |
| Rejected | 0 | 0 | match |

## Step 6: bars against the manual queries

| Check | Expected | Seen | Result |
|---|---|---|---|
| Chart sits under the KPI tiles | yes | yes | match |
| Heading | ROI by Channel | ROI by Channel | match |
| Bar order, top to bottom | Email, Search, Display, Social | Email, Search, Display, Social | match |
| Left-out note | none shown | none shown | match |

| Channel | Expected label | Bar shows | Manual query ROI | Result |
|---|---|---|---|---|
| Email | 155.1% | 155.1% | matches the bar | match |
| Search | 131.4% | 131.4% | matches the bar | match |
| Display | 46.4% | 46.4% | matches the bar | match |
| Social | 24.8% | 24.8% | matches the bar | match |

The developer ran the four Channel queries in `manual-roi-queries.sql` and reported that each query's ROI matches its bar. Expected query figures (spend / revenue / ROI %): email 2875.19 / 7333.83 / 155.1; search 18451.80 / 42691.58 / 131.4; display 10178.85 / 14897.67 / 46.4; social 13750.29 / 17155.23 / 24.8.

The KPI tiles in the same screenshot also show the clean file's totals ($45,256.13 spend, $82,078.31 revenue, 81.4% Overall ROI).

Every bar matches its expected label and its manual query.

Screenshot: `images/roibychannelchartclean.png`

## Not checked here

The no-spend, negative ROI and "Channel ROI uses totals, not an average" cases are covered only by the automated tests in `verification-report-tests.md`. No made-up rows went into the real database.
