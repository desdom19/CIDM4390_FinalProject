# 07: Automated tests against the test database

**What to build:** An xUnit test project that runs integration tests through the one seam, `CsvImportService`, against a separate `campaign_analytics_test` database. Each test feeds a CSV stream to the service and asserts on the returned `ImportSummary` and on the rows actually in the database. The test run prints actual against expected figures and saves them as a verification report under `.scratch/csv-import/`.

**Blocked by:** 03: Duplicate Daily Results; 04: File-level rejections (and 05: Channel mismatch rejection, for the optional test)

**Status:** ready-for-agent

**Spec:** `.scratch/csv-import/spec.md`

## Setup (each step needs developer approval first; nothing is installed or changed in a database without asking)

- A new xUnit test project (xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk) referencing the app.
- A `campaign_analytics_test` database with the same channels / campaigns / results schema and the four seeded Channels, and `campaign_app_user` granted access to it. A draft script is at `.scratch/csv-import/setup-test-database.sql`, for the developer to run as root.
- The test project's own User Secrets holding the test connection string, set by the developer, so the password never passes through the agent.

## Acceptance criteria

- [ ] Guard: the test run fails immediately unless the connection string's database name is exactly `campaign_analytics_test`.
- [ ] Each test empties `results` and `campaigns` in the test database first. Channels are not touched.
- [ ] `campaign_analytics` (the real database) is not touched.
- [ ] Test (from 01): importing the clean CSV into empty tables reports 240 rows read, 240 imported, 0 duplicates, 0 rejected. The results table holds 240 rows, and the campaigns table holds 8 rows, each with `start_date` 2026-09-01, `end_date` 2026-09-30 and NULL `roi_goal`.
- [ ] Test (from 01): a Campaign named with different letter case from an existing Campaign is matched to it, not created again.
- [ ] Test (from 02): a Channel written in different letter case (e.g. `Search`) is matched to the seeded Channel and the row imports, while a name that isn't one of the four seeded Channels (e.g. `radio`) is rejected with the right line number and reason.
- [ ] Test (from 02): a small test CSV in which every row of one Campaign is invalid. That Campaign is not created, every one of its rows is reported as rejected with the right line number and reason, and the other Campaigns' rows import normally.
- [ ] Test (from 03): re-importing the clean CSV. The first import gives 240 imported. The second gives 240 read, 0 imported, 240 duplicates, 0 rejected, and the results table still holds 240 rows.
- [ ] Test (from 03): importing the error CSV into empty tables reports 241 read, 236 imported, 1 duplicate and 4 rejected. The results table holds 236 rows, and there are 8 Campaigns, each dated 2026-09-01 to 2026-09-30.
- [ ] Test (from 03): the error CSV's rejected rows are exactly:
  - [ ] line 45: invalid date `2026-09-31`
  - [ ] line 126: invalid date `2026-13-05`
  - [ ] line 169: empty spend
  - [ ] line 221: empty clicks
- [ ] Test (from 03): the duplicate skipped in the error CSV is line 202 (Brand Keywords Search, 2026-09-10, first seen at line 71).
- [ ] Test (from 04): a header with a missing column is refused with the right message, and the results and campaigns tables stay empty.
- [ ] Test (from 04): an empty file is refused, and the tables stay empty.
- [ ] Test (from 04): a reordered, differently-cased header imports normally.
- [ ] Optional test (from 05): a small test CSV with a Channel mismatch row reports that row as rejected with the expected reason, and the other rows import normally.
- [ ] The test output prints actual against expected figures for every assertion, saved as a verification report under `.scratch/csv-import/`.
