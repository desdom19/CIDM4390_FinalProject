# 01: Tracer bullet: import the clean CSV end to end

**What to build:** A developer clicks "Import CSV" in the navigation, uploads `sample-data/campaign_results_clean.csv` through a form, and sees a summary of the rows read and imported. Behind the page, `CsvImportService` parses the file with CsvHelper, looks up each Channel's `channel_id` by name, finds or creates each Campaign by name (matched the way MySQL's case-insensitive collation compares names; new Campaigns get a NULL ROI Goal), inserts every Daily Result, and recalculates each affected Campaign's start and end dates from the earliest and latest dates of its Daily Results. All of this happens in one transaction.

This slice assumes valid input. Validation, duplicates and file-level rejections come in later tickets. Automated tests and their setup come in ticket 07.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

**Spec:** `.scratch/csv-import/spec.md`

## Setup (each step needs developer approval first)

- CsvHelper added to the app project. (Done: CsvHelper 34.1.0, approved 2026-11-21.)

## PBI acceptance criteria delivered (verbatim from issue #7)

- [ ] Importing the clean sample CSV file returns that 240 rows were imported into the empty results SQL table with 8 rows in the campaigns SQL table.
- [ ] After importing the clean sample CSV file, each campaign's start_date should be 2026-09-01 and end_date should be 2026-09-30.

## Acceptance criteria

- [ ] An "Import CSV" link in the site navigation opens the import page.
- [ ] The import page has a file upload form protected by an anti-forgery token.
- [ ] Submitting a file calls `CsvImportService` (Stream in, `ImportSummary` out) and shows the file name, rows read and imported count on the same page.
- [ ] The connection string comes from configuration (User Secrets) through dependency injection.
- [ ] Each Daily Result is linked to the Channel's `channel_id`, found by Channel name using MySQL's case-insensitive collation (`Search` finds `search`).
- [ ] Campaigns are found by name using MySQL's case-insensitive collation (the lookup runs in MySQL, so `brand keywords search` finds `Brand Keywords Search`). A missing Campaign is created on its first row, with that row's Channel, a NULL `roi_goal`, and that row's date as its start and end date.
- [ ] After all rows, each affected Campaign's `start_date` / `end_date` equal the MIN / MAX `result_date` of its Daily Results.
- [ ] All database work for one import runs in a single transaction.
- [ ] Manual check (developer): with `results` and `campaigns` emptied, upload `sample-data/campaign_results_clean.csv` through the page and verify by SQL that `results` holds 240 rows and `campaigns` holds 8 rows, each with `start_date` 2026-09-01, `end_date` 2026-09-30 and NULL `roi_goal`.
