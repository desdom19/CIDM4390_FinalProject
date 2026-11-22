# 05: Channel mismatch rejection (optional)

**What to build:** A Campaign always runs on exactly one Channel. A row that names an existing Campaign (in the database, or created earlier in the same import) with a different Channel is rejected, with a reason naming both Channels, e.g. "Campaign 'Brand Keywords Search' belongs to channel 'search', not 'social'". The Campaign's Channel is set by its first accepted row (or is already stored) and never changes.

This ticket is optional. Neither sample file contains this case, and no PBI acceptance criterion depends on it.

**Blocked by:** 02: Row validation and the Rejected rows table

**Status:** ready-for-agent

**Spec:** `.scratch/csv-import/spec.md`

## Acceptance criteria

- [ ] A row whose Channel differs from its Campaign's stored Channel is rejected and appears in the Rejected rows table with its line number.
- [ ] The reason names the Campaign, its Channel, and the Channel given on the row.
- [ ] Two rows in one file that give a new Campaign different Channels: the first valid row sets the Channel, and the later conflicting row is rejected.
- [ ] Rejected mismatch rows don't change the Campaign's Channel or dates.
- [ ] Campaigns are matched as in 01 (MySQL's case-insensitive collation) when checking which Channel a Campaign belongs to.

Automated tests for this ticket are in 07.
