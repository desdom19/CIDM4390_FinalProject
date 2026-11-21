# PBI 3: Sample Campaign Data for CSV Import and Dashboard

Status: ready-for-agent
Branch: 5-pbi-3-feature-sample-campaign-data-creation

## Problem Statement

The developer is building Campaign Performance Analytics, an ASP.NET Core + MySQL app with a CSV import and a dashboard. The database schema (channels, campaigns, results) exists and the four Channels are seeded. But there is no data to import, nothing to build the dashboard against, and no deliberately broken file to test that the import rejects or reports bad input. Without realistic, known-good and known-bad sample files, the developer can't build or demo the import and dashboard features, or tell whether they're correct.

## Solution

Add two sample CSV files to a `sample-data` folder at the repository root:

1. **Clean file**: 240 Daily Results covering 8 Campaigns (2 per Channel across email, social, search and display) for 30 consecutive days, with realistic marketing numbers. 6 Campaigns are profitable and 2 are Weak Campaigns.
2. **Error file**: the same data with exactly 5 known defects: 2 bad dates, 2 blank values (1 spend, 1 clicks) and 1 duplicate row, for 241 rows.

The data is produced with Claude agentic coding. The developer reviews the result against the acceptance criteria, using a verification report that prints the actual figures behind each criterion.

## User Stories

1. As a developer, I want a clean sample CSV of Daily Results, so that I can build and test the CSV import against known-good input.
2. As a developer, I want the CSV columns to be `campaign_name, channel, date, spend, impressions, clicks, leads, revenue`, so that each row maps directly onto one row of the results table plus its Campaign and Channel.
3. As a developer, I want exactly one header row followed by data rows, so that the import can rely on a simple header-then-data structure.
4. As a developer, I want exactly 240 data rows in the clean file, so that I can confirm the import loaded every row.
5. As a developer, I want all four seeded Channels (email, social, search, display) represented, so that every Channel shows up on the dashboard.
6. As a developer, I want Channel values spelled exactly like the seeded `channel_name` values (lowercase), so that the import can find each Channel by name without normalising it.
7. As a developer, I want exactly 2 Campaigns per Channel and 8 distinct Campaigns in total, so that per-Channel comparisons on the dashboard are balanced.
8. As a developer, I want descriptive, unique Campaign names ending in their Channel, so that Campaigns are easy to tell apart on the dashboard.
9. As a developer, I want Campaign names without commas, so that the CSV needs no quoting and stays simple to parse.
10. As a developer, I want every Campaign to have exactly one Daily Result for each of the 30 days from 2026-09-01 to 2026-09-30, so that time-series charts have no gaps and the totals are comparable.
11. As a developer, I want dates in ISO `yyyy-MM-dd` format, so that parsing doesn't depend on culture.
12. As a developer, I want spend and revenue as plain decimals with 2 places and no currency symbols or thousands separators, so that they parse straight into decimal columns.
13. As a developer, I want impressions, clicks and leads as plain non-negative integers, so that they parse straight into int columns.
14. As a developer, I want clicks never to exceed impressions in any Daily Result, so that the data is plausible.
15. As a developer, I want leads never to exceed clicks in any Daily Result, so that the funnel is plausible.
16. As a developer, I want revenue to be 0 whenever leads are 0, so that revenue is always backed by leads.
17. As a developer, I want each Channel to have its own realistic rates (e.g. search with a higher CTR than display, email with a high CTR and low spend), so that Channel comparisons on the dashboard look real.
18. As a developer, I want day-to-day variation and a slight weekend dip, so that trend charts look like real campaign performance rather than flat lines.
19. As a developer, I want 6 Campaigns whose 30-day total revenue is strictly greater than total spend, so that the dashboard shows mostly profitable Campaigns.
20. As a developer, I want 2 Weak Campaigns whose 30-day total revenue is lower than total spend, so that I can check that the dashboard and goal alerts highlight underperformers.
21. As a developer, I want the two Weak Campaigns to be in different Channels (Influencer Promo Social and Awareness Banner Display), so that no Channel looks uniformly bad.
22. As a developer, I want strong Campaigns to land at roughly ROI 0.50–3.00 and Weak Campaigns at roughly ROI −0.40 to −0.15 over 30 days, so that the ROI spread is realistic and varied.
23. As a developer, I want individual days of a strong Campaign to be allowed to lose money, so that the daily data looks realistic while the 30-day totals still meet the criteria.
24. As a developer, I want an error CSV built from the clean file, so that every defect is known and isolated against a known-good baseline.
25. As a developer, I want exactly 2 bad dates in the error file that fail to parse under any culture (`2026-09-31` and `2026-13-05`), so that I can test date validation without depending on culture.
26. As a developer, I want exactly 1 blank spend value and 1 blank clicks value, written as truly empty fields, so that I can test handling of missing required values (as opposed to valid zeros).
27. As a developer, I want exactly 1 duplicate row, an exact copy of a clean row placed in a different Campaign's block (not next to its original), so that I can test that the import finds duplicates anywhere in the file, not just in neighbouring rows.
28. As a developer, I want each of the 5 defects on a different row, and the duplicate copied from a defect-free row, so that each defect triggers exactly one validation error.
29. As a developer, I want the error file to have exactly 241 data rows, so that the row count reflects the single added duplicate.
30. As a developer, I want rows sorted by Campaign, then date, in both files, so that the files are easy to read and compare by eye.
31. As a developer, I want both files saved as UTF-8 without a BOM, so that the first header name isn't corrupted by a stray BOM.
32. As a developer, I want a verification report listing per-Campaign totals (spend, revenue, ROI) and the actual count behind every acceptance criterion, so that I can review Claude's output against the requirements myself rather than trust a plain pass or fail.
33. As a developer, I want the report to show the row number and value of each injected defect in the error file, so that I know exactly what the import should flag.
34. As a developer, I want ROI calculated as (revenue − spend) ÷ spend, so that the sample data is judged the same way the dashboard and goal alerts will judge it.
35. As a developer, I want both CSVs committed to the `sample-data` folder on the PBI 3 branch, so that they're versioned and available to the import and dashboard work.
36. As a developer, I want the glossary committed separately from the data, so that I can include or drop it from the PR on its own.

## Implementation Decisions

- **Deliverables:** two CSV files in a new `sample-data` folder at the repo root, named `campaign_results_clean.csv` and `campaign_results_errors.csv`.
- **Columns:** exactly `campaign_name,channel,date,spend,impressions,clicks,leads,revenue`. The CSV doesn't include the Campaign fields `start_date`, `end_date` and `roi_goal`. How the import fills those in (e.g. start/end from the min/max date) is left to the import PBI.
- **Channels:** `email`, `social`, `search`, `display`, all lowercase, matching the seeded `channel_name` values exactly.
- **Campaigns:**
  - email: Fall Newsletter Email, Cart Reminder Email
  - social: Back to School Social, Influencer Promo Social (**Weak**)
  - search: Brand Keywords Search, Competitor Keywords Search
  - display: Retargeting Display, Awareness Banner Display (**Weak**)
- **Date range:** 2026-09-01 through 2026-09-30. Every Campaign has exactly one Daily Result per day.
- **Format:** UTF-8 without BOM, one header row, comma-separated, no quoted fields, ISO dates, money to 2 decimal places, counts as integers.
- **Ordering:** sorted by Campaign, then date.
- **Rules for every row in the clean file:** clicks ≤ impressions; leads ≤ clicks; leads = 0 ⇒ revenue = 0.
- **How realism is modelled:** each Channel has its own ranges for daily spend, CTR (clicks ÷ impressions), lead rate (leads ÷ clicks) and value per lead. Daily values get random noise and a weekend dip. Revenue comes from leads × value per lead ± noise.
- **Profitability:** judged on each Campaign's 30-day totals using ROI = (revenue − spend) ÷ spend. Strong Campaigns: total revenue strictly greater than total spend, targeting ROI of about 0.50–3.00. Weak Campaigns: total revenue lower than total spend, targeting ROI of about −0.40 to −0.15. Individual days don't have to meet these targets.
- **How the error file is built:** a copy of the clean file with defects on 5 different rows:
  - date `2026-09-31` on one row and `2026-13-05` on another, replacing the valid date in place
  - an empty `spend` field on one row and an empty `clicks` field on another
  - an exact copy of one defect-free row, inserted inside a different Campaign's block
- **Generation:** done with Claude agentic coding, using a seeded (reproducible) generator script kept outside the repo in the session scratchpad. Only the CSVs are committed.
- **Commits:** on branch `5-pbi-3-feature-sample-campaign-data-creation`. Each CSV gets its own commit referencing #5, made after the developer reviews its verification report. `.scratch/` (spec and tickets), `GLOSSARY.md`, and the agent-skills configuration each go in separate commits made by the developer. Commit messages are in past tense.

## Testing Decisions

- **What makes a good test here:** check only the finished files on disk, exactly as the import will see them. Never inspect the generator or its parameters. A check passes or fails based on what is in the CSVs.
- **Seam:** one seam, the two committed CSV files. A throwaway verification script (kept in the scratchpad, not committed) reads both files and checks each acceptance criterion.
- **Show the actual figures, not just pass or fail.** For each criterion the script prints the measured values next to the expected ones:
  - Clean file row count (expected 240) and error file row count (expected 241), header excluded.
  - Distinct Channels and Campaigns per Channel (expected 4 × 2 = 8 distinct Campaigns), and the number of Daily Results per Campaign (expected 30 each, covering 2026-09-01 → 2026-09-30).
  - The number of rows breaking clicks ≤ impressions, leads ≤ clicks, and leads = 0 ⇒ revenue = 0 (expected 0 each).
  - A per-Campaign table of total spend, total revenue and ROI, flagged strong (revenue **strictly greater** than spend) or weak (revenue < spend). Expected: 6 strong, 2 weak, and the weak ones are Influencer Promo Social and Awareness Banner Display.
  - Error file: the number of unparseable dates (expected 2), blank spend fields (expected 1), blank clicks fields (expected 1) and exact duplicate rows (expected 1), each listed with its row number and value. The check also confirms the 5 defects are on 5 different rows and that every other row is identical to the clean file.
  - Format checks: no BOM, header matches the column list exactly, channel values all lowercase.
- **Who reviews:** the developer reviews the report against the acceptance criteria before committing.
- **Prior art:** none. There are no tests in the codebase yet, and the earlier PBIs were checked by hand with SQL (`DESCRIBE`, `SHOW CREATE TABLE`, `SELECT`), as recorded in the data schema notes.

## Out of Scope

- The CSV import feature itself (parsing, validation messages, duplicate handling, mapping to the channels/campaigns/results tables).
- Filling in Campaign `start_date`, `end_date` and `roi_goal` values.
- The dashboard and the goal alerts.
- Loading the sample data into MySQL.
- Committing the generator or verification scripts, and adding a test project to the solution.
- Error types beyond those listed (e.g. negative numbers, clicks > impressions, unknown Channels, extra or missing columns, quoted fields).

## Further Notes

- **ROI and ROAS are different measures:** ROI = (revenue − spend) ÷ spend, where 1.00 means a 100% return, and `roi_goal` is an ROI target, not ROAS. The dashboard and goal alerts must use the same ROI formula. See `GLOSSARY.md`.
- `GLOSSARY.md` was created while this spec was being worked out and defines Channel, Campaign, Daily Result, ROI, ROI Goal, ROAS and Weak Campaign.
