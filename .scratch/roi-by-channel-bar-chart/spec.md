# PBI 6: ROI by Channel Bar Chart

Status: ready-for-agent
Branch: 11-pbi-6-feature-roi-by-channel-bar-chart
Issue: #11

## Problem Statement

The Dashboard page shows the Overall ROI of every Campaign together, but the marketer can't see which Channel brings back the most and which brings back the least. A single overall figure hides a Channel that loses money behind one that earns a lot. To compare the Channels today, the marketer would have to write SQL by hand, grouping Daily Results by Channel and computing each Channel's ROI from its totals. That is slow, needs database access, and makes mistakes easy, such as averaging each Campaign's ROI instead of using the Channel's total revenue and total spend.

## Solution

A horizontal bar chart headed "ROI by Channel", placed under the KPI tiles on the Dashboard page.
- **Bars:** one bar per Channel (email, social, search, display) that has spend. Each bar's length shows that Channel's ROI, calculated from the Channel's total revenue and total spend.
- **Labels:** each bar is labelled with its ROI as a percentage to one decimal, formatted the same way as the Overall ROI tile.
- **Order:** bars are sorted from highest to lowest ROI, so the top and bottom bars show at a glance which Channel returns the most and which the least.
- **Negative ROI:** a negative bar grows left from a zero line.
- **No spend:** a Channel with no spend is left out, and a short note names it.
- **No Daily Results:** the chart area shows no bars and a short message with a link to the Import CSV page.
- **Daily Results but no spend:** the chart area shows no bars and a short message saying ROI can't be shown.
- **Database failure:** the page's single error message replaces both the tiles and the chart.

## User Stories

1. As a marketer, I want a bar chart of ROI by Channel on the Dashboard page, so that I can compare the Channels without writing SQL.
2. As a marketer, I want the chart placed under the KPI tiles, so that the overall figures come first and the Channel breakdown follows.
3. As a marketer, I want the chart headed "ROI by Channel", so that I know what it shows.
4. As a marketer, I want one bar for each of the four Channels (email, social, search and display) that has spend, so that every Channel I'm paying for is compared.
5. As a marketer, I want each Channel's ROI calculated as (the Channel's total revenue − its total spend) ÷ its total spend, so that it matches the glossary's definition of ROI.
6. As a marketer, I want each Channel's ROI calculated from that Channel's totals and never as an average of its Campaigns' ROIs, so that a small Campaign with a high ROI can't hide a large one losing money on the same Channel.
7. As a marketer, I want the totals of each Channel to cover every Daily Result of every Campaign on that Channel, so that the chart shows all-time performance like the tiles do.
8. As a marketer, I want the bars horizontal, with the Channel name on the left, so that the names stay readable on a phone.
9. As a marketer, I want the Channel names shown capitalised (Email, Social, Search, Display), so that they read as labels.
10. As a marketer, I want the bars sorted from highest to lowest ROI, so that the top bar is the Channel returning the most and the bottom bar the one returning the least.
11. As a marketer, I want the order decided by the exact ROI, not the rounded label, so that two Channels showing the same rounded label are still in the right order.
12. As a marketer, I want Channels with exactly the same ROI ordered by name A to Z, so that the order is always predictable.
13. As a marketer, I want each bar's length in proportion to its ROI, so that I can compare the Channels by eye.
14. As a marketer, I want the longest bar to fill its side of the chart, so that the chart uses the space available.
15. As a marketer, I want each bar labelled with its ROI as a percentage rounded to one decimal (e.g. 155.1%), so that I can read the exact figure.
16. As a marketer, I want the labels formatted exactly like the Overall ROI tile, so that the dashboard reads consistently: commas, one decimal, midpoints rounded away from zero, a minus sign on a negative, and never "-0.0%".
17. As a marketer, I want each label placed just past the end of its bar, so that a short bar's label is never clipped.
18. As a marketer, I want a Channel that loses money shown as a bar growing left from a zero line, so that a loss looks different from a gain.
19. As a marketer, I want a negative label shown with a minus sign (e.g. -30.0%) on the left of its bar, so that the loss is unmistakable.
20. As a marketer, I want the zero line at the left edge when no Channel has a negative ROI, so that an all-positive chart uses the full width.
21. As a marketer, I want the zero line partway across when some Channels gain and some lose, with positive and negative bars on the same scale, so that the lengths stay comparable.
22. As a marketer, I want a Channel with an ROI of exactly 0 to show its "0.0%" label at the zero line with no bar, so that break-even is shown honestly.
23. As a marketer, I want every bar in the same neutral colour, so that the chart reports figures without judging them before ROI Goals and Weak Campaign alerts exist.
24. As a marketer, I want a Channel whose total spend is exactly 0 left out of the chart, so that I'm never shown a meaningless division by zero.
25. As a marketer, I want a Channel with no Daily Results at all left out the same way, so that only Channels with spend are compared.
26. As a marketer, I want a Channel with some spend shown even if it has no revenue (an ROI of -100.0%), so that a Channel losing everything isn't hidden.
27. As a marketer, I want a muted note under the chart naming any Channel left out, e.g. "Not shown (no spend): Display.", so that a missing bar doesn't look like a bug.
28. As a marketer, I want that note shown only when at least one Channel is left out, so that it doesn't clutter a full chart.
29. As a marketer, I want the chart area to show no bars and the message "No Daily Results yet. Import a CSV to see ROI by Channel.", with "Import a CSV" linking to the Import CSV page, when there are no Daily Results, so that I know why the chart is empty and what to do.
30. As a marketer, I want the chart area to show no bars and the message "No Channel has any spend yet, so ROI can't be shown." when there are Daily Results but no Channel has any spend, so that I'm not sent to import data that is already there.
31. As a marketer, I want the page's single database error message to replace both the tiles and the chart when the database can't be reached, so that I'm never shown half the data or false figures.
32. As a marketer, I want the chart and the tiles read from the same snapshot of the data, so that the four Channels' spend always adds up to Total Spend, even if an import runs while the page loads.
33. As a marketer using a screen reader, I want each row readable as text (e.g. "Email 155.1%"), so that the chart makes sense without seeing it.
34. As a marketer, I want the chart to show email 155.1%, search 131.4%, display 46.4% and social 24.8%, in that order, after the clean sample CSV is imported, so that I can trust it.
35. As a developer, I want the chart's sorting, scaling, zero line and left-out list worked out by a pure builder, so that they can be tested quickly without a database.
36. As a developer, I want the view only to loop over what the builder gives it, so that no chart logic hides in markup.
37. As a developer, I want the per-Channel figures tested against the test database, so that the query, the ROI and the labels are checked together.
38. As a developer, I want an automated test showing that a Channel's ROI uses its totals and not an average of its Campaigns, so that the glossary's rule is protected for Channels too.
39. As a developer, I want the existing zero-spend dashboard test extended to check the chart, with those chart checks written to this PBI's verification report, so that the "Daily Results but no spend" state is checked against real data and the evidence sits with this PBI.
40. As a developer, I want the database-failure test to show that no totals and no Channel figures come back, so that a failure can never be shown as data.
41. As a developer, I want this PBI's test figures saved in a verification report in its own scratch folder, while the CSV import and KPI tile reports stay in theirs, so that each PBI keeps its own evidence.
42. As a developer, I want a file of manual SQL queries, one separate SELECT per Channel, so that I can check each bar against the real database myself.
43. As a developer, I want to see the empty chart by eye in the real app before re-importing the clean file, so that I've seen the empty state for real.
44. As a developer, I want screenshots of the empty and clean dashboards, and the actual figures from the manual check recorded next to the expected ones, so that the acceptance criteria are backed by evidence.

## Implementation Decisions

- **Domain language:** use the glossary's vocabulary (Channel, Campaign, Daily Result, ROI) in code and UI text. The glossary's ROI entry already covers a Channel, always computed from total revenue and total spend. No glossary changes. "Bar chart" and "left-out Channel" are UI terms and stay out of the glossary.
- **No ADR:** every decision here is cheap to change later.
- **Rendering:** the chart is server-rendered HTML and CSS with Bootstrap only. There is no JavaScript and no chart library. Every label is a real string from the formatter that can be read on the page and tested.
- **Modules:**
  - **`DashboardService`**:
    - **One public method:** `GetKpiTotalsAsync()` is replaced by `GetDashboardAsync()`. It returns a `DashboardResult` holding both the KPI totals and the per-Channel totals.
    - **One snapshot:** the existing totals query and a new per-Channel query run on one connection, inside one read-only transaction at `REPEATABLE READ` (MySQL's default level). The tiles and the chart therefore come from the same snapshot.
    - **Per-Channel query:** it starts from `channels` and LEFT JOINs `campaigns` and `results`, grouping by Channel. Each Channel's spend and revenue sums are wrapped in COALESCE, so all four Channels always come back, with 0 when a Channel has no Daily Results.
    - **Errors:** a `MySqlException` from either query is caught and logged, and an error result is returned, as today.
    - Existing callers and tests of `GetKpiTotalsAsync()` are updated to the new method. Their checks don't change.
  - **`DashboardResult`**: either the totals and the Channel totals, or an error message, never both.
  - **`ChannelTotals`** (new): a Channel's stored name, total spend (`decimal`) and total revenue (`decimal`). Its ROI is a `decimal?` calculated in C# as (revenue − spend) ÷ spend, the same way as `KpiTotals.OverallRoi`. It is `null` when total spend is exactly 0.
  - **`RoiChart`** (new): a pure static builder. It takes the Channel totals and whether there are any Daily Results, and returns everything the view draws:
    - **State:** one of three.
      - *Bars:* at least one Channel has spend.
      - *No Daily Results:* there are no Daily Results at all.
      - *No spend:* there are Daily Results, but every Channel's total spend is 0.
    - **Bars:** one per Channel with spend, sorted by exact ROI from highest to lowest, with exact ties broken by stored Channel name using ordinal A to Z. Each bar has:
      - the display name: the stored name with its first letter capitalised, using the invariant culture
      - the label: `KpiFormatter.Roi` of the ROI
      - whether it is negative
      - its width: a percentage of the chart track
    - **Scale:** positive and negative bars share one scale, the largest positive ROI plus the largest negative ROI's absolute value. Each bar's width is its ROI's absolute value ÷ that sum × 100, so the longest bar on each side reaches its edge.
    - **Zero line:** its position across the track is the largest negative ROI's absolute value ÷ the same sum × 100.
      - 0 (the left edge) when no ROI is negative.
      - 100 (the right edge) when every ROI is negative.
      - The left edge, with every bar at width 0, when every ROI shown is exactly 0.
    - **Exactly 0:** an ROI of exactly 0 has width 0 and draws no bar. Its label sits at the zero line. There is no minimum bar width.
    - **Left-out Channels:** the display names of Channels with total spend exactly 0, in ordinal A-to-Z order of stored name. They are listed in every state, but the note shows only in the *Bars* state.
  - **`KpiFormatter`**: unchanged. The chart uses `KpiFormatter.Roi` for its labels.
  - **`DashboardController`**: unchanged in shape. It calls the new service method and passes the result to the view.
  - **Dashboard view:** under the tiles, a section headed "ROI by Channel".
    - **Bars state:**
      - Each row shows the Channel name, a track, the bar and the label.
      - Bars are one neutral Bootstrap colour. A positive bar starts at the zero line and grows right, with its label to its right. A negative bar ends at the zero line and grows left, with its label to its left. The track leaves room for the labels so they are never clipped.
      - Each row reads as text ("Email 155.1%") and the bar itself is `aria-hidden`.
      - Widths and positions go into inline styles using the invariant culture, so a decimal point is never written as a comma.
      - When any Channel is left out, a muted note follows the bars: "Not shown (no spend): " + the names joined by ", " + ".".
    - **No Daily Results state:** no bars, and "No Daily Results yet. Import a CSV to see ROI by Channel.", with "Import a CSV" linking to the Import CSV page. The tiles' existing hint above the tiles stays.
    - **No spend state:** no bars, and "No Channel has any spend yet, so ROI can't be shown." with no link.
    - **Error result:** the existing error message replaces both the tiles and the chart.
- **Schema:** no schema changes.
- **Installs:** none expected; xUnit and MySqlConnector are already installed. If one turns out to be needed, stop and ask the developer first.
- **Database changes:** none expected beyond each test emptying `results` and `campaigns` in `campaign_analytics_test`, which is already established. Any change to `campaign_analytics` needs the developer's separate, explicit approval at the time.

## Testing Decisions

- **What makes a good test:** test only external behaviour through the seams. Assert on the figures, strings, order, widths and states that come out, not on SQL text, private helpers or markup.
- **Seams:**
  1. **`DashboardService.GetDashboardAsync()`**: integration tests against `campaign_analytics_test`. These tests also pass the returned Channel totals through `RoiChart` and check the exact labels, order and state.
  2. **`RoiChart`**: pure unit tests, with no database.
  - There are no controller, view or HTTP tests, as in PBI 4 and PBI 5. The chart's placement, look and messages are checked by hand.
- **Test-first:** each ticket is built test-first with its own tests and adds its results to this PBI's verification report.
- **Database tests**, in the existing single "Test database" collection (a second collection would run in parallel and empty tables during another test). Each test empties `results` and `campaigns` first:
  1. **Clean file:** import the clean sample CSV through `CsvImportService`.
     - Expected Channel totals:

       | Channel | Spend | Revenue | ROI |
       |---|---|---|---|
       | email | 2,875.19 | 7,333.83 | ≈ 1.5507 |
       | social | 13,750.29 | 17,155.23 | ≈ 0.2476 |
       | search | 18,451.80 | 42,691.58 | ≈ 1.3137 |
       | display | 10,178.85 | 14,897.67 | ≈ 0.4636 |

     - The four spends add up to Total Spend (45,256.13).
     - Expected bars in order: Email 155.1%, Search 131.4%, Display 46.4%, Social 24.8%.
     - No Channel is left out.
  2. **Empty tables:** state *No Daily Results*, no bars, and all four Channels in the left-out list.
  3. **A Channel with no Daily Results:** a small inline CSV with Daily Results for email, social and search only. Display is left out and named in the left-out list, and the other three have bars.
  4. **A Channel with a negative ROI:** a small inline CSV where one Channel spends more than it earns. Expected: its label has a minus sign, it is marked negative, and it sorts below the positive Channels.
  5. **Channel ROI uses totals, not an average:** a small inline CSV with two Campaigns on the same Channel, one spending $100 and earning $300 and another spending $10,000 and earning $5,000. Expected: that Channel shows -47.5%, not the +75% average of their ROIs.
  6. **Database failure:** the existing unreachable-database test (port 1, short timeout) also checks that neither totals nor Channel totals come back. It touches no database.
  7. **Zero total spend (extends PBI 5's existing test):** the existing test keeps its tile checks in the KPI tiles report, unchanged. It also starts a second set of checks tagged with this PBI's folder and ticket 03. Those checks are: state *No spend*, no bars, and all four Channels in the left-out list. The test calls `AssertAll()` on both sets, so a chart mismatch fails it and shows as FAIL in this PBI's report.
- **`RoiChart` unit tests:**
  - bars are sorted by exact ROI, including two ROIs that round to the same label
  - exact ties are broken by name A to Z
  - the zero line is at 0 when every ROI is 0 or above
  - the zero line is partway across, at the right position, when the values are mixed
  - the zero line is at 100 when every ROI is negative
  - the longest bar on each side reaches its edge
  - an ROI of exactly 0 has width 0
  - every ROI shown being exactly 0 gives the zero line at 0 and no division error
  - Channels with total spend 0 are left out and listed A to Z
  - *No Daily Results* and *No spend* states give no bars
  - display names are capitalised
  - widths and positions are calculated correctly regardless of the current culture
- **Verification report:**
  - Add a `ReportFeature` for this PBI, so that its tests write `verification-report-tests.md` under `.scratch/roi-by-channel-bar-chart/`.
  - The KPI tiles feature's seam text is updated to the new service method name.
  - After the last automated ticket, run the **full** test suite and confirm that all three reports (csv-import, dashboard-kpi-tiles, roi-by-channel-bar-chart) are rewritten with a new run time.
- **Manual check against `campaign_analytics`:**
  - Only the developer touches the real database.
  - The agent prepares `manual-roi-queries.sql`: a read-only row count, plus four separate SELECTs, one per Channel filtered by Channel name. That is deliberately not the app's grouped query. Each SELECT returns the Channel's spend, revenue and ROI × 100 rounded to one decimal. The agent also prepares a manual verification report template.
  - Order of steps:
    1. The developer runs the row count.
    2. If the clean file isn't the only data loaded, the developer gives separate, explicit approval to empty `results` and `campaigns` (Channels are not touched).
    3. The developer checks the empty chart message and its Import CSV link by eye, and takes a screenshot.
    4. The developer re-imports the clean sample CSV through the Import CSV page.
    5. The developer compares each bar with its query and takes a screenshot.
    6. After the manual check, the developer writes `roi-by-channel-data-flow.md`. The agent never writes it.
  - The figures are recorded in `verification-report-manual.md`, and the screenshots go in the folder's `images/`.
  - The no-spend, negative and totals-not-average cases are covered only by the automated tests on the test database. No made-up rows go into the real database.
- **Prior art:**
  - PBI 5's `DashboardServiceTests` (the `TestDatabase` fixture, inline CSVs through `CsvImportService`, the unreachable-database test) and `KpiFormatterTests`.
  - The per-feature `VerificationReport`.
  - PBI 5 ticket 04's developer-run manual check.

## Out of Scope

- Date, Channel or Campaign filters, and per-Campaign breakdowns.
- ROAS, ROI Goals, Weak Campaign alerts, and green/red or per-Channel colours.
- Hover tooltips, animation, and any JavaScript chart library.
- Trends and comparisons with earlier periods.
- Controller, view or HTTP automated tests.
- Schema changes.
- `roi-by-channel-data-flow.md`, the PBI's knowledge artifact: a data flow diagram from `results` through `campaigns` and `channels` to the bar chart. The developer writes it after the manual check. The agent never writes it.
- The PBI 6 entry in `agent-log.md` (the developer writes it).
- Commits, GitHub issues and pull requests (the developer handles all of these).

## Further Notes

- **Expected figures:** summed straight from the clean sample CSV, each Channel has 60 Daily Results and no row has zero spend. The ROIs are email 155.07%, social 24.76%, search 131.37% and display 46.36%, which show as the acceptance criteria's 155.1%, 24.8%, 131.4% and 46.4%. None is near a rounding midpoint.
- **Current real data:** `campaign_analytics` should still hold the clean file (240 Daily Results) after PBI 5's manual check. The manual check still starts with a read-only row count.
- **Permissions:** ask before any install and before any change to `campaign_analytics`.
- **Deliverables:** leave everything uncommitted. The developer commits in past tense, referencing #11.

## PBI Acceptance Criteria (verbatim from issue #11)

- A bar chart is displayed under the KPI tiles in the dashboard page.
- With the clean sample CSV imported, each bar percentage displays as the following:
email: 155.1%
social: 24.8%
search: 131.4%
display: 46.4%
- If the results table is empty, the chart area shows no bars and a short message and link to Import CSV page.

Developer's own task (the knowledge artifact):

- roi-by-channel-data-flow.md is committed to the repository, and displays a data flow diagram that shows how the data flows from results through campaigns and channels to the bar chart.
