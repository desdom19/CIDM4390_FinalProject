1# 02: Sorting, mixed scaling and negative bars

**What to build:** The marketer sees at a glance which Channel returns the most and which the least.
- **Sorted:** bars are sorted by exact ROI from highest to lowest. Channels with exactly the same ROI are ordered by stored name, A to Z.
- **Negative bars:** a Channel that loses money is drawn as a bar growing left from a zero line, with its label (e.g. -30.0%) on the left of the bar.
- **Shared scale:** positive and negative bars share one scale.
  - The zero line is at the left edge when no ROI is negative.
  - It is partway across when the values are mixed.
  - It is at the right edge when every ROI is negative.

After this ticket, the clean CSV reads Email 155.1%, Search 131.4%, Display 46.4%, Social 24.8%, from top to bottom.

**Blocked by:** 01: Tracer bullet: ROI by Channel bars for the clean CSV

**Status:** done

**Spec:** `.scratch/roi-by-channel-bar-chart/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #11)

- [ ] With the clean sample CSV imported, each bar percentage displays as the following:
email: 155.1%
social: 24.8%
search: 131.4%
display: 46.4%

## Acceptance criteria

- [ ] **Sorting:** `RoiChart` orders bars by exact ROI (not the rounded label) from highest to lowest. Exact ties are broken by stored Channel name, ordinal A to Z.
- [ ] **Scale:** the scale is the largest positive ROI plus the largest negative ROI's absolute value.
  - Each bar's width is its ROI's absolute value ÷ that sum × 100.
  - The zero line's position is the largest negative ROI's absolute value ÷ that sum × 100.
  - When every ROI shown is exactly 0, the zero line is at 0, every width is 0, and nothing divides by zero.
- [ ] **Negative bars:** each bar records whether it is negative. In the view, a positive bar starts at the zero line and grows right with its label on its right. A negative bar ends at the zero line and grows left with its label on its left. An ROI of exactly 0 still draws no bar, with its label at the zero line.
- [ ] **Clean-file database test (extended from 01):** the bars come back in the order Email 155.1%, Search 131.4%, Display 46.4%, Social 24.8%, and none is negative.
- [ ] **Negative-Channel database test:** a small inline CSV where one Channel spends more than it earns. Expected:
  - its label has a minus sign
  - it is marked negative
  - it sorts below the positive Channels
  - the zero line is partway across
- [ ] **Totals-not-average database test (extended from 01):** the -47.5% Channel is marked negative.
- [ ] **`RoiChart` unit tests:**
  - sorting by exact ROI, including two ROIs that round to the same label
  - exact ties ordered A to Z
  - the zero line partway across at the right position for mixed values
  - the zero line at 100 when every ROI is negative
  - the longest bar on each side reaching its edge
  - every ROI exactly 0 giving the zero line at 0 with no division error
- [ ] **Test run:** the full suite passes, and this PBI's report holds the new results.
- [ ] **Manual check (developer):** with the clean data loaded, the bars read Email, Search, Display, Social from top to bottom.
- [ ] **Permissions:** no installs and no changes to `campaign_analytics`. If either becomes necessary, stop and ask the developer first.
