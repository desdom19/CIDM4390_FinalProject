# 03: Left-out Channels, the empty state and the database error

**What to build:** The chart behaves sensibly when there is little or no data.
- **No spend:** a Channel whose total spend is exactly 0, whether or not it has Daily Results, is left out of the chart. A muted note under the bars names it, e.g. "Not shown (no spend): Display.".
- **No Daily Results:** the chart area shows no bars and "No Daily Results yet. Import a CSV to see ROI by Channel.", with "Import a CSV" linking to the Import CSV page.
- **Daily Results but no spend:** the chart area shows no bars and "No Channel has any spend yet, so ROI can't be shown." with no link.
- **Database failure:** the page's single error message replaces both the tiles and the chart.

**Blocked by:** 01: Tracer bullet: ROI by Channel bars for the clean CSV. It is worked after 02 only because tickets are done one at a time.

**Status:** done

**Spec:** `.scratch/roi-by-channel-bar-chart/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #11)

- [ ] If the results table is empty, the chart area shows no bars and a short message and link to Import CSV page.

## Acceptance criteria

- [ ] **States:** `RoiChart` returns one of three states.
  - *Bars:* at least one Channel has spend.
  - *No Daily Results:* there are no Daily Results.
  - *No spend:* there are Daily Results, but every Channel's total spend is 0.
- [ ] **Left-out list:** `RoiChart` lists the display names of Channels with total spend exactly 0, ordered by stored name A to Z, in every state. A Channel with some spend and no revenue (-100.0%) is shown, not left out.
- [ ] **View, *Bars* state:** when at least one Channel is left out, a muted note follows the bars: "Not shown (no spend): " + the names joined by ", " + ".". No note when none is left out.
- [ ] **View, *No Daily Results* state:** no bars, and "No Daily Results yet. Import a CSV to see ROI by Channel.", with "Import a CSV" linking to the Import CSV page. The tiles' existing hint stays.
- [ ] **View, *No spend* state:** no bars, and "No Channel has any spend yet, so ROI can't be shown." with no link and no left-out note.
- [ ] **Error result:** the existing error message replaces both the tiles and the chart, and the failure is logged.
- [ ] **Empty-tables database test:** state *No Daily Results*, no bars, and all four Channels in the left-out list.
- [ ] **No-Daily-Results-Channel database test:** a small inline CSV with Daily Results for email, social and search only. Display is in the left-out list, and the other three have bars.
- [ ] **Database-failure test (extended from PBI 5):** the unreachable-database test (port 1, short timeout) also checks that neither totals nor Channel totals come back. It touches no database.
- [ ] **Zero-total-spend test (extended from PBI 5):**
  - The existing test keeps its tile checks in the KPI tiles report, unchanged.
  - It starts a second set of checks tagged with `roi-by-channel-bar-chart` and ticket 03, titled e.g. "Zero total spend: chart shows the no-spend message (extends PBI 5's zero-spend test)". Those checks are: state *No spend*, no bars, and all four Channels in the left-out list.
  - It calls `AssertAll()` on both sets, so a chart mismatch fails the test and shows as FAIL in this PBI's report.
  - `VerificationReport` itself doesn't change.
- [ ] **`RoiChart` unit tests:**
  - Channels with total spend 0 are left out and listed A to Z
  - a Channel with spend and no revenue is shown at -100.0%
  - the *No Daily Results* and *No spend* states give no bars
- [ ] **Final test run:** the **full** suite is run.
  - All tests pass.
  - All three reports (csv-import, dashboard-kpi-tiles, roi-by-channel-bar-chart) are rewritten with a new run time.
  - The zero-spend chart checks appear in this PBI's report.
- [ ] **Permissions:** no installs and no changes to `campaign_analytics`. If either becomes necessary, stop and ask the developer first.
