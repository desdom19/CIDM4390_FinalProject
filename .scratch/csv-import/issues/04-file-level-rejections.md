# 04: File-level rejections

**What to build:** A file that isn't a valid Daily Results CSV is refused as a whole, with one clear message and nothing written to the database: no file chosen, an empty file, a file that isn't `.csv`, a file over 5 MB, or a header with a missing or extra column. If the database fails part-way through an import, the transaction rolls back and the page says the import failed and nothing was saved.

**Blocked by:** 01: Tracer bullet: import the clean CSV end to end

**Status:** ready-for-agent

**Spec:** `.scratch/csv-import/spec.md`

## Acceptance criteria

- [ ] Submitting the form with no file shows a clear message, and nothing is imported.
- [ ] Files without a `.csv` extension are refused with a clear message.
- [ ] Files over 5 MB are refused, with a request size limit on the upload.
- [ ] An empty file (no header) is refused with a clear message.
- [ ] The header must contain exactly `campaign_name, channel, date, spend, impressions, clicks, leads, revenue`, matched by name, in any order and any letter case.
- [ ] A header with a missing column is refused with a message naming the missing column.
- [ ] A header with an extra column is refused with a message naming the extra column.
- [ ] For any file-level refusal, nothing is written to the database and the summary shows only the error message (no counts).
- [ ] A database error during the import rolls back the whole transaction, and the page shows that the import failed and nothing was saved.
- [ ] Manual check (developer): submitting with no file, a `.txt` file, and a CSV missing a header column each show the right message, and the row counts in `results` and `campaigns` don't change.

Automated tests for this ticket are in 07.
