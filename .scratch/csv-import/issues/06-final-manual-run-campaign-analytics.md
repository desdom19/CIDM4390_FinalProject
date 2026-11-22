# 06: Final manual run against campaign_analytics

**What to build:** An end-to-end demonstration of the PBI's acceptance criteria in the real app and the real database. Both sample CSVs are uploaded through the Import CSV page into `campaign_analytics`, and the summaries and table contents are recorded with their actual figures.

The developer then writes and commits `csv-import-notes.md`. If 07 is finished, the notes describe the automated tests. If it isn't, they describe the manual checks from 01–04.

Only the developer touches `campaign_analytics`: in the manual checks in 01–04 and in this run. Automated tests never do. Emptying its `results` and `campaigns` tables is a database change that needs the developer's separate, explicit approval at the time.

**Blocked by:** 03: Duplicate Daily Results; 04: File-level rejections

**Runs after (when possible, not blocking):** 07: Automated tests against the test database

**Status:** ready-for-human

**Spec:** `.scratch/csv-import/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #7)

- [ ] csv-import-notes.md is committed to the repository and states how the import process works, what CsvHelper does, validation rules, and how the import was tested.

## Acceptance criteria

- [ ] Developer approval is given before `results` and `campaigns` in `campaign_analytics` are emptied. Channels are not touched.
- [ ] **Clean file run:**
  - [ ] Starting from empty tables, uploading `sample-data/campaign_results_clean.csv` through the page reports 240 imported, 0 duplicates and 0 rejected.
  - [ ] SQL check: the results table holds 240 rows, and the campaigns table holds 8 rows, each with `start_date` 2026-09-01 and `end_date` 2026-09-30.
- [ ] **Error file run:**
  - [ ] After emptying the tables again (with approval), uploading `sample-data/campaign_results_errors.csv` reports 236 imported, 1 duplicate and 4 rejected.
  - [ ] The Rejected rows table shows lines 45, 126, 169 and 221 with their reasons.
  - [ ] SQL check: the results table holds 236 rows.
- [ ] The actual figures from both runs (page summaries and SQL query results) are recorded in the verification report under `.scratch/csv-import/`.
- [ ] Afterwards, the developer decides which data to leave loaded for the dashboard work.
