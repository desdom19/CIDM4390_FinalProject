# 02: Row validation and the Rejected rows table

**What to build:** Bad rows no longer reach the database. Each row is trimmed and checked against every validation rule. A row that fails any rule is rejected, and all its reasons are recorded. The summary page shows a rejected count and a Rejected rows table that lists each rejected row by its file line number (header = line 1) with its reasons. Valid rows still import normally. A Campaign is only created from a valid row, so a Campaign whose rows are all invalid never appears.

**Blocked by:** 01: Tracer bullet: import the clean CSV end to end

**Status:** ready-for-agent

**Spec:** `.scratch/csv-import/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #7)

- [ ] CSV import summary for the error sample CSV file returns the rejected row numbers and reasons for rejection.

## Acceptance criteria

- [ ] Every field is trimmed before it is checked. A field that is empty or only spaces counts as empty.
- [ ] A row is rejected if any of these fail:
  - [ ] the row has exactly 8 fields
  - [ ] no field is empty
  - [ ] the date is exact `yyyy-MM-dd`, parsed with the invariant culture (`2026-09-31`, `2026-13-05`, `09/14/2026` all rejected)
  - [ ] spend and revenue are decimals ≥ 0
  - [ ] impressions, clicks and leads are whole numbers ≥ 0 (`12.5`, `abc`, `-3` rejected)
  - [ ] the Channel matches one of the four seeded `channel_name`s, using MySQL's case-insensitive collation (`Search` is accepted as `search`; only a name that isn't a seeded Channel, such as `radio`, is rejected)
- [ ] Plausibility rules (clicks ≤ impressions, leads ≤ clicks, revenue 0 when leads are 0) are **not** enforced.
- [ ] A wrong field count rejects only that row, not the whole file.
- [ ] A row with several problems counts once as rejected, and all its reasons are listed.
- [ ] Rejected rows never affect Campaign dates and never create a Campaign.
- [ ] `ImportSummary` carries the rejected count and a list of rejected rows (line number and reasons).
- [ ] The summary page shows the rejected count and a Rejected rows table with columns **Line** and **Reason(s)**, where Line is the file line number (header = line 1).
- [ ] Manual check (developer): the clean-file upload from 01 still reports 0 rejected.

Automated tests for this ticket are in 07.
