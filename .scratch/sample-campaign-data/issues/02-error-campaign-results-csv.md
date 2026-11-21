# 02: Error campaign results CSV

**What to build:** A second sample CSV in `sample-data/` with exactly five known defects, so the developer can test that the CSV import catches bad dates, missing values and duplicates. It's an exact copy of the clean file with the defects injected. Each defect is on its own row, so each one should trigger exactly one validation error. It's checked by a throwaway verification script that compares the finished file against the clean file, and committed after the developer reviews the report. See the parent spec: `.scratch/sample-campaign-data/spec.md`.

**Blocked by:** 01: Clean campaign results CSV

**Status:** ready-for-agent

## Defects

- Bad date `2026-09-31` (a day that doesn't exist), replacing one row's date in place
- Bad date `2026-13-05` (a month that doesn't exist), replacing another row's date in place
- One row with an empty `spend` field (truly empty, not `0`/`NULL`/whitespace)
- One row with an empty `clicks` field (truly empty)
- One exact copy of a defect-free row, inserted inside a **different** Campaign's block (not next to its original)

## Acceptance criteria

- [ ] The file `campaign_results_errors.csv` exists in `sample-data/`, UTF-8 without BOM, with the same header and format as the clean file
- [ ] There are 241 data rows, not counting the header
- [ ] Exactly 2 dates fail to parse: `2026-09-31` and `2026-13-05`
- [ ] Exactly 2 blank values: 1 in `spend`, 1 in `clicks`
- [ ] Exactly 1 duplicate row, which copies a defect-free row and isn't next to its original
- [ ] The 5 defects are on 5 different rows
- [ ] Every other row is identical to the corresponding row of the clean file
- [ ] The verification report prints the actual counts next to the expected ones, plus the row number and value of each defect (and, for the duplicate, the row number of its original), and the developer has reviewed it
- [ ] The file is committed on `5-pbi-3-feature-sample-campaign-data-creation`, with a commit message referencing #5
