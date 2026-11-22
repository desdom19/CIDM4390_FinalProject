# 03: Duplicate Daily Results

**What to build:** Importing never doubles the figures. A valid row is skipped as a **Duplicate Daily Result** when its Campaign already has a Daily Result for that date, either in the database or from an earlier accepted row in the same file. The first one stands, even if the duplicate's figures differ. The summary page shows how many duplicates were skipped (a count only, no list). This makes both sample files import exactly as the PBI's acceptance criteria expect, and makes an accidental re-import harmless.

**Blocked by:** 02: Row validation and the Rejected rows table

**Status:** ready-for-agent

**Spec:** `.scratch/csv-import/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #7)

- [ ] Importing the error sample CSV file returns that 236 rows were imported into the empty results SQL table, 4 rows were rejected, and 1 was skipped as a duplicate.

## Acceptance criteria

- [ ] Duplicates are keyed on Campaign + date. Whether the figures match doesn't matter.
- [ ] Duplicates are checked against existing results in the database **and** rows already accepted earlier in the same file, wherever they appear in the file.
- [ ] Only rows that passed validation are checked for duplicates. A rejected row is never counted as a duplicate, and a duplicate is never counted as rejected.
- [ ] Duplicate rows are not inserted and don't affect Campaign dates.
- [ ] `ImportSummary` carries a duplicate count, and the summary page shows it as "Duplicates skipped" (no table).
- [ ] Duplicates use the same Campaign matching as 01 (MySQL's case-insensitive collation), so a row naming `brand keywords search` on a date that `Brand Keywords Search` already has is a duplicate.
- [ ] Manual check (developer): uploading the clean CSV a second time reports 0 imported and 240 duplicates, and the results table still holds 240 rows.

Automated tests for this ticket (clean re-import, error CSV counts, rejected lines 45 / 126 / 169 / 221, duplicate at line 202) are in 07.
