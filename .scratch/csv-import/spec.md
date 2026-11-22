# PBI 4: CSV Import of Daily Results

Status: ready-for-agent
Branch: 7-pbi-4-feature-csv-import
Issue: #7

## Problem Statement

The developer is building Campaign Performance Analytics, an ASP.NET Core MVC + MySQL app. The `campaign_analytics` database has the channels, campaigns and results tables, with the four Channels seeded, and PBI 3 produced two sample CSVs of Daily Results (a clean file of 240 rows and an error file of 241 rows with 5 known defects). But there is no way to get that data into the database, so the dashboard has nothing to read. Loading it by hand would be slow and error-prone, and bad input (invalid dates, blank values, Duplicate Daily Results) would either break the load or quietly corrupt the data.

## Solution

An "Import CSV" page in the app where the developer uploads a Daily Results CSV. The import reads the file with CsvHelper, validates every row, finds or creates each Campaign by name, skips Duplicate Daily Results, and writes the valid rows to MySQL in one transaction. It then shows a summary: how many rows were imported, how many Duplicate Daily Results were skipped, how many rows were rejected, and a table of every rejected row with its file line number and the reasons.

## User Stories

1. As a developer, I want an "Import CSV" link in the site navigation, so that I can reach the import page from anywhere in the app.
2. As a developer, I want a page with a file upload form, so that I can import a CSV without using SQL tools.
3. As a developer, I want the form protected by an anti-forgery token, so that another site can't submit imports on my behalf.
4. As a developer, I want only `.csv` files up to 5 MB accepted, so that wrong or oversized files are refused before any processing.
5. As a developer, I want a clear message when I submit the form without choosing a file, so that I know why nothing happened.
6. As a developer, I want an empty file rejected with a clear message, so that I don't get a meaningless "0 imported" result.
7. As a developer, I want the header checked for exactly the columns `campaign_name, channel, date, spend, impressions, clicks, leads, revenue`, so that a file in the wrong format is refused as a whole.
8. As a developer, I want header columns matched by name regardless of order and letter case, so that a reordered or differently-cased header still imports.
9. As a developer, I want a header with a missing or extra column rejected with a message naming the column, so that I can fix the file quickly.
10. As a developer, I want file-level problems to reject the whole upload with nothing written, so that a wrong file never partly loads.
11. As a developer, I want the CSV parsed with CsvHelper, so that parsing is handled by a proven library rather than hand-written string splitting.
12. As a developer, I want every field trimmed before it is checked, so that stray spaces don't cause false rejections.
13. As a developer, I want a row with any empty field (or a field of only spaces) rejected, so that incomplete Daily Results never enter the database.
14. As a developer, I want dates accepted only in exact `yyyy-MM-dd` form, parsed culture-independently, so that `2026-09-31`, `2026-13-05` and ambiguous formats like `09/14/2026` are rejected.
15. As a developer, I want spend and revenue to be decimals of 0 or more, so that non-numeric or negative money values are rejected.
16. As a developer, I want impressions, clicks and leads to be whole numbers of 0 or more, so that values like `12.5`, `abc` or `-3` are rejected.
17. As a developer, I want the channel to match a seeded Channel name regardless of letter case (using MySQL's case-insensitive collation), with only a name that isn't one of the four seeded Channels rejected, so that unknown or misspelled Channels are rejected rather than guessed at.
18. As a developer, I want the import to look up the `channel_id` from the Channel name, so that results are linked to the right Channel without me knowing the IDs.
19. As a developer, I want a row with the wrong number of fields rejected as a single row, so that one malformed line doesn't stop the rest of the file from importing.
20. As a developer, I want every problem on a row listed, not just the first, so that I can fix the row in one pass.
21. As a developer, I want a row with several problems counted once as rejected, so that the rejected count equals the number of bad rows.
22. As a developer, I want plausibility rules (clicks ≤ impressions, leads ≤ clicks, revenue 0 when leads are 0) not enforced, so that the import only rejects data that is invalid, not data that is merely unusual.
23. As a developer, I want Campaigns found by name (after trimming), matched the way MySQL's case-insensitive collation compares names, so that each Daily Result is attached to the right Campaign and a difference in letter case never creates a second Campaign.
24. As a developer, I want a missing Campaign created automatically on its first valid row, so that I don't have to set up Campaigns before importing.
25. As a developer, I want a Campaign never created when none of its rows are valid, so that no empty Campaigns appear and the NOT NULL start and end dates always have real values.
26. As a developer, I want newly created Campaigns to have no ROI Goal (NULL), so that the import never invents a goal nobody set.
27. As a developer, I want a row rejected when its Channel differs from the Channel of the Campaign it names, with a reason naming both Channels, so that a Campaign always runs on exactly one Channel.
28. As a developer, I want a Campaign's Channel fixed by its first accepted row (or its stored Channel), so that later conflicting rows can't change it.
29. As a developer, I want a row rejected for validation never to be counted as a duplicate, so that each bad row is reported for its real problem.
30. As a developer, I want a valid row skipped as a Duplicate Daily Result when its Campaign already has a Daily Result for that date in the database, so that re-importing doesn't double the figures.
31. As a developer, I want a valid row skipped as a Duplicate Daily Result when an earlier row in the same file already supplied that Campaign and date, so that duplicates anywhere in the file are caught, not just adjacent ones.
32. As a developer, I want the first Daily Result for a Campaign and date to stand even if a later duplicate has different figures, so that the outcome is predictable.
33. As a developer, I want re-importing the clean file to import 0 rows and skip all 240 as duplicates, so that an accidental second import is harmless.
34. As a developer, I want each affected Campaign's start and end dates recalculated from the earliest and latest dates of its Daily Results at the end of the import, so that the date range always matches the data.
35. As a developer, I want rejected and duplicate rows to have no effect on Campaign dates, so that a bad date like `2026-09-31` can never stretch a Campaign's range.
36. As a developer, I want the whole import written in one database transaction, so that a database failure part-way through leaves nothing half-imported.
37. As a developer, I want a clear "import failed, nothing was saved" message on a database failure, so that I know it is safe to retry.
38. As a developer, I want the summary to show the file name, so that I know which file the results belong to.
39. As a developer, I want the summary to show counts of rows read, imported, Duplicate Daily Results skipped, and rejected, so that I can check the import against what I expected.
40. As a developer, I want a Rejected rows table listing each rejected row's file line number (header is line 1) and its reasons, labelled as line numbers, so that I can open the file and go straight to the problem.
41. As a developer, I want importing the clean sample CSV into empty tables to report 240 imported and leave 8 Campaigns, so that I can confirm every row loaded.
42. As a developer, I want each Campaign's dates to be 2026-09-01 to 2026-09-30 after importing the clean file, so that the dashboard's date ranges are right.
43. As a developer, I want importing the error sample CSV into empty tables to report 236 imported, 4 rejected and 1 duplicate, so that I can confirm validation and duplicate handling.
44. As a developer, I want the error file's rejected rows reported as lines 45, 126, 169 and 221 with their reasons, so that the summary matches the known defects from PBI 3.
45. As a developer, I want automated tests run against a separate test database, so that testing never touches my real data.
46. As a developer, I want test results printed with the actual figures and saved under `.scratch/`, so that I can review the evidence rather than trust a plain pass or fail.

## Implementation Decisions

- **Domain term:** a **Duplicate Daily Result** (added to `GLOSSARY.md`) is a Daily Result for a Campaign and date that already has one. The first one stands and the duplicate is not imported, even if its figures differ. Use glossary vocabulary (Channel, Campaign, Daily Result, ROI Goal) throughout code and UI text.
- **New dependency:** CsvHelper, added to the app project. It is not yet installed, so ask the developer before installing.
- **Modules:**
  - **`CsvImportService`**: one public method that takes the uploaded file's `Stream` (and file name) and returns an `ImportSummary`. It owns parsing, validation, Campaign lookup and creation, duplicate detection, inserts and date recalculation. Its connection string comes from configuration (User Secrets) through dependency injection. This is a deep module and the only test seam.
  - **`ImportSummary`**: the result. It holds the file name, rows read, imported count, duplicate count, rejected count, a list of rejected rows (file line number and one or more reasons), and a file-level error message when the whole upload was refused (in which case the counts aren't shown).
  - **`ImportController`**: thin. GET shows the upload form. POST checks the anti-forgery token, the file's presence, the `.csv` extension and the 5 MB limit, calls the service, and renders the summary on the same page.
  - **Layout:** add an "Import CSV" navigation link.
- **Processing order:**
  1. File-level checks: file present, non-empty, `.csv`, ≤ 5 MB, header has exactly the 8 required columns (matched by name, case-insensitive, any order, no extras). Any failure refuses the whole upload with one message and writes nothing.
  2. Per row: trim every field, then collect every validation failure:
     - wrong field count
     - empty field
     - date not exact `yyyy-MM-dd` (invariant culture)
     - spend or revenue not a decimal ≥ 0
     - impressions, clicks or leads not a whole number ≥ 0
     - Channel not one of the seeded `channel_name`s (matched with MySQL's case-insensitive collation)
     - the Campaign exists (in the database or created earlier in this import) on a different Channel
     
     Any failure rejects the row, with all reasons recorded.
  3. Only valid rows are checked for duplicates, keyed on Campaign + date, against both existing results and rows already accepted from this file.
  4. Campaigns are looked up by name in MySQL, inside the import's transaction, so the `campaign_name` column's case-insensitive collation decides whether two names match. This also covers Campaigns created earlier in the same import. A Campaign that doesn't exist is created when its first valid, non-duplicate row is reached: name, Channel, `roi_goal` NULL, and that row's date as both start and end (the columns are NOT NULL).
  5. After all rows: recalculate `start_date` / `end_date` for every affected Campaign from MIN/MAX of its `result_date`.
- **Transaction:** all database work for one import runs in a single transaction. A database error rolls everything back, and the page reports that nothing was saved. Invalid and duplicate rows never cause a rollback.
- **Row numbers:** reported as file line numbers, with the header as line 1 (data row N = line N + 1), and labelled "Line" in the UI.
- **Summary page:** file name, counts (rows read, imported, duplicates skipped, rejected), and a Rejected rows table (Line, Reason(s)). Duplicates are shown as a count only.
- **Schema:** no schema changes. `campaigns.roi_goal` must accept NULL. If it turns out to be NOT NULL, stop and ask the developer before changing anything.

## Testing Decisions

- **What makes a good test:** test only external behaviour through the seam. Feed a CSV stream to `CsvImportService`, then assert on the returned `ImportSummary` and on the rows actually in the database (counts and Campaign dates). Don't assert on internal helpers, SQL text or CsvHelper configuration.
- **Seam:** one seam, `CsvImportService`. No HTTP or controller tests.
- **Test project:** a new xUnit test project (packages xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, all needing developer approval before installing). It runs integration tests against a separate database, `campaign_analytics_test`, with the same schema and the four seeded Channels. Creating that database and granting `campaign_app_user` access are database changes that need the developer's approval first.
- **Test connection string:** stored in the test project's own User Secrets. The developer sets it themselves, so the password never passes through the agent. A guard fails the test run immediately unless the connection string's database name is exactly `campaign_analytics_test`.
- **Isolation:** each test empties `results` and `campaigns` in the test database before running. Channels are left alone.
- **When:** all test setup and automated tests are delivered in the last ticket (07). Earlier tickets are checked manually by the developer through the page.
- **Required tests:**
  1. **Clean sample CSV:** 240 rows read, 240 imported, 0 duplicates, 0 rejected; 240 rows in results; 8 Campaigns, each dated 2026-09-01 to 2026-09-30, with NULL `roi_goal`.
  2. **Error sample CSV:** 241 read, 236 imported, 1 duplicate, 4 rejected; rejected lines 45 (bad date `2026-09-31`), 126 (bad date `2026-13-05`), 169 (empty spend) and 221 (empty clicks), each with its reason; 8 Campaigns dated 09-01 to 09-30.
  3. **Re-import of the clean file:** import it, then import it again. The second import reports 0 imported and 240 duplicates, and results still holds 240 rows.
  4. **All rows of one Campaign invalid:** a small test CSV where every row of one Campaign fails validation. That Campaign is not created, its rows are all reported as rejected, and the other Campaigns import normally.
- **Optional test:** Channel mismatch. A row naming an existing Campaign with a different Channel is rejected with a reason naming both Channels.
- **Reporting:** the test run prints the actual figures next to the expected ones for every assertion, and saves them as a verification report under `.scratch/csv-import/`.
- **Final manual run:** an end-to-end upload of both sample files through the page against the real `campaign_analytics`, which needs `results` and `campaigns` emptied first. This happens only with the developer's separate, explicit approval. Otherwise `campaign_analytics` is never touched.
- **Prior art:** none. There are no tests in the codebase. Earlier PBIs were checked by hand with SQL, and PBI 3 used a verification report printing actual against expected figures, which this PBI's report follows.

## Out of Scope

- `csv-import-notes.md`: the developer writes it.
- Setting or editing ROI Goals, and editing existing Campaigns other than recalculating their dates.
- Showing a list of Campaigns created or a table of duplicate rows in the summary.
- Plausibility checks (clicks ≤ impressions, leads ≤ clicks, revenue 0 when leads are 0).
- Normalising Channel or Campaign name spelling or case.
- Updating existing Daily Results from a re-import (duplicates are always skipped).
- The dashboard, ROI calculations and goal alerts.
- Controller or UI automated tests.
- Schema changes, including a unique constraint on Campaign + date.
- Commits, GitHub issues and pull requests (the developer handles all of these).

## Further Notes

- **Expected error-file outcome:** 241 data rows − 4 rejected − 1 duplicate = 236 imported. The duplicate is line 202, a copy of line 71 (Brand Keywords Search, 2026-09-10).
- **ROI:** (revenue − spend) ÷ spend, per `GLOSSARY.md`. The import stores no ROI, but `roi_goal` is an ROI value, not ROAS.
- **Permissions:** ask before each install (CsvHelper, the xUnit packages) and before each database change (creating `campaign_analytics_test`, granting access, emptying tables in `campaign_analytics` for the final run).
- **Deliverables:** leave everything uncommitted. The developer commits in past tense, referencing #7.
