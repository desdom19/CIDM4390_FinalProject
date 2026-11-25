# PBI 5: Dashboard KPI Tiles

Status: ready-for-agent
Branch: 9-pbi-5-feature-dashboard-kpi-tiles-creation
Issue: #9

## Problem Statement

The marketer can now load Daily Results into `campaign_analytics` through the Import CSV page, but the app has nowhere to see them. To find out how the Campaigns are performing overall (how much was spent, how much revenue came back, how many leads, impressions and clicks were won, and the ROI of everything together), they would have to write SQL by hand. That is slow, needs database access, and makes mistakes easy, such as averaging each Campaign's ROI instead of computing ROI from the totals.

## Solution

A Dashboard page, reached from a "Dashboard" link in the site navigation, showing six KPI tiles: Total Spend, Total Revenue, Total Leads, Total Impressions, Total Clicks and Overall ROI. The five totals are summed by SQL over every Daily Result. Overall ROI is calculated from those totals as (revenue − spend) ÷ spend. Money is shown as dollars with commas, counts with commas, and ROI as a percentage to one decimal. When total spend is 0, ROI shows "N/A (spend is 0)". When there are no Daily Results, the tiles show zeros and a hint links to the Import CSV page. When the database can't be reached, the page shows an error message instead of tiles, never zeros.

## User Stories

1. As a marketer, I want a "Dashboard" link in the site navigation, so that I can reach the dashboard from any page.
2. As a marketer, I want the Dashboard link placed between Home and Import CSV, so that it is easy to find and the Home page stays as it is.
3. As a marketer, I want the Dashboard link to open the Dashboard page, so that one click shows me how my Campaigns are performing.
4. As a marketer, I want the page titled "Dashboard", so that I know where I am.
5. As a marketer, I want six KPI tiles, Total Spend, Total Revenue, Total Leads, Total Impressions, Total Clicks and Overall ROI, so that I see overall performance at a glance.
6. As a marketer, I want the tiles in that order, so that the cost and return figures come first and ROI sums them up.
7. As a marketer, I want each tile to show its label in small text and its value in large text, so that the figure stands out.
8. As a marketer, I want the tiles laid out three per row on a desktop, two on a tablet and one on a phone, so that they're readable on any screen.
9. As a marketer, I want the tiles in neutral colours, so that the page reports figures without judging them before ROI Goals and Weak Campaign alerts exist.
10. As a marketer, I want the totals to cover every Daily Result of every Campaign and Channel, so that the dashboard shows overall performance.
11. As a marketer, I want no date, Channel or Campaign filters on this page, so that the figures are always the all-time totals.
12. As a marketer, I want Total Spend to be the sum of spend over all Daily Results, so that I know everything I have spent.
13. As a marketer, I want Total Revenue to be the sum of revenue over all Daily Results, so that I know everything my Campaigns have earned.
14. As a marketer, I want Total Leads, Total Impressions and Total Clicks to be the sums of those figures over all Daily Results, so that I know the overall reach and response.
15. As a marketer, I want the totals calculated by SQL on the results table, so that they come straight from the stored Daily Results.
16. As a marketer, I want all five totals read in one query, so that the tiles always come from the same snapshot of the data and can't disagree with each other.
17. As a marketer, I want Overall ROI calculated as (total revenue − total spend) ÷ total spend, so that it matches the glossary's definition of ROI.
18. As a marketer, I want Overall ROI calculated from the totals and never as an average of each Campaign's ROI, so that a small Campaign with a high ROI can't hide a large one losing money.
19. As a marketer, I want Overall ROI calculated from the same totals the spend and revenue tiles show, so that the three tiles always agree.
20. As a marketer, I want Total Spend and Total Revenue shown as dollars with commas and two decimals (e.g. $45,256.13), so that money reads naturally.
21. As a marketer, I want Total Leads, Total Impressions and Total Clicks shown as whole numbers with commas (e.g. 4,995,183), so that large counts are easy to read.
22. As a marketer, I want Overall ROI shown as a percentage rounded to one decimal (e.g. 81.4%), so that the return is easy to understand.
23. As a marketer, I want an ROI exactly halfway between two values rounded up in size (81.35% shows as 81.4%), so that rounding is predictable.
24. As a marketer, I want a negative ROI shown with a minus sign (e.g. -12.3%), so that I can see at once when my Campaigns lose money overall.
25. As a marketer, I want an ROI that rounds to zero always shown as 0.0%, never -0.0%, so that break-even doesn't look like a loss.
26. As a marketer, I want the formatting to be the same on every machine, whatever the server's regional settings, so that I always see "$" and "81.4%" rather than another currency sign or "81.4 %".
27. As a marketer, I want Overall ROI shown as "N/A (spend is 0)" when total spend is 0, so that I'm never shown a meaningless division by zero.
28. As a marketer, I want Overall ROI shown as "N/A (spend is 0)" when total spend is 0 even if there is revenue, while the revenue tile still shows its real total, so that the revenue isn't hidden.
29. As a marketer, I want the "N/A" rule to depend only on total spend being exactly 0, so that the rule is simple and predictable.
30. As a marketer, I want the tiles to show $0.00, $0.00, 0, 0, 0 and "N/A (spend is 0)" when there are no Daily Results, so that an empty database reads as zero, not as an error.
31. As a marketer, I want a short hint, "No Daily Results yet. Import a CSV to see your totals.", with a link to the Import CSV page when there are no Daily Results, so that I know why the tiles are zero and what to do.
32. As a marketer, I want the hint shown only when there are no Daily Results, so that it doesn't clutter a dashboard that has data.
33. As a marketer, I want a clear message ("The dashboard couldn't load because of a database error. Try again later.") instead of the tiles when the database can't be reached, so that I'm never shown false zeros.
34. As a developer, I want a database failure on the dashboard logged, so that I can find out what went wrong.
35. As a marketer, I want the dashboard to show the clean sample CSV's figures exactly ($45,256.13, $82,078.31, 1,630, 4,995,183, 37,882 and 81.4%) after that file is imported, so that I can trust the tiles.
36. As a developer, I want the figure calculations and the formatting tested automatically against the test database, so that a change can't quietly break a tile.
37. As a developer, I want the formatting rules tested without a database, so that every rounding, negative and N/A case is checked quickly.
38. As a developer, I want an automated test showing that Overall ROI uses the totals and not an average, so that the glossary's rule is protected.
39. As a developer, I want an automated test of the database failure that touches no database at all, so that it can never harm real data.
40. As a developer, I want the dashboard tests' actual figures saved in a verification report in the dashboard's own scratch folder, while the CSV import report stays in its folder, so that each PBI keeps its own evidence.
41. As a developer, I want the full test suite run after the verification report change, with both report locations checked, so that I know the CSV import tests and their report still work.
42. As a developer, I want a file of manual SQL queries, one separate SELECT per tile plus an ROI query, so that I can check each tile against the real database myself.
43. As a developer, I want to see the empty dashboard by eye after the real tables are emptied and before the clean file is re-imported, so that I've seen the empty state in the real app.
44. As a developer, I want the actual figures from the manual check recorded next to the expected ones in a manual verification report, so that the acceptance criteria are backed by evidence.

## Implementation Decisions

- **Domain language:** use the glossary's vocabulary (Daily Result, Campaign, Channel, ROI) in code and UI text. The glossary's ROI entry now covers "all campaigns together", always computed from total revenue and total spend, with "average ROI" listed under _Avoid_. "KPI tile" is a UI term and stays out of the glossary.
- **No ADR:** every decision here is cheap to change later.
- **Modules:**
  - **`DashboardService`**: a deep module with one public method, `GetKpiTotalsAsync()`, returning a `DashboardResult`. It runs one SQL query on `results` that sums spend, revenue, leads, impressions and clicks, each wrapped in COALESCE so that an empty table gives 0. Its connection string comes from configuration (User Secrets) through dependency injection, the same way as `CsvImportService`. It catches a `MySqlException`, logs it and returns an error result instead of throwing.
  - **`DashboardResult`**: either the totals or an error message, never both. It follows the same pattern as `ImportSummary.Failure`.
  - **`KpiTotals`**: the raw figures. Total spend and total revenue are `decimal`. Total leads, impressions and clicks are `long`, because the sum of an `int` column can go past the `int` range. It also holds whether there are any Daily Results, which the empty-state hint uses. Overall ROI is a `decimal?` calculated in C# from total revenue and total spend, and it is `null` when total spend is exactly 0.
  - **`KpiFormatter`**: a pure static class that turns the figures into display text. It uses the invariant culture with explicit patterns:
    - money: "$" followed by the value with commas and 2 decimals
    - counts: commas and no decimals
    - ROI: the value × 100 with commas and 1 decimal, followed by "%"
    - a `null` ROI: "N/A (spend is 0)"
    - an ROI that rounds to zero: always "0.0%", never "-0.0%"

    It must not use culture-dependent currency formatting, which can show another currency sign, or invariant percent formatting, which puts a space before the %. Midpoints round away from zero, which is .NET's default for `decimal`.
  - **`DashboardController`**: thin. Its `Index` action at `/Dashboard` calls the service and passes the result to the view.
  - **Dashboard view:** six Bootstrap cards in a responsive grid (3 / 2 / 1 per row) in the order Total Spend, Total Revenue, Total Leads, Total Impressions, Total Clicks, Overall ROI. Each card has its label in small text above its value in large text, and the colours are neutral. The view only displays strings from `KpiFormatter`.
    - **No Daily Results:** the hint and its Import CSV link appear above the tiles, which still show their zero values.
    - **Error result:** the error message replaces the tiles.
  - **Layout:** add a "Dashboard" navigation link between Home and Import CSV. The Home page is unchanged.
- **Schema:** no schema changes.
- **Installs:** none expected; xUnit and MySqlConnector are already installed. If one turns out to be needed, stop and ask the developer first.
- **Database changes:** none expected beyond each test emptying `results` and `campaigns` in `campaign_analytics_test`, which is already established. Any change to `campaign_analytics` needs the developer's separate, explicit approval at the time.

## Testing Decisions

- **What makes a good test:** test only external behaviour through the seams. Assert on the figures and strings that come out, not on SQL text, private helpers or markup.
- **Seams:**
  1. **`DashboardService`**: integration tests against `campaign_analytics_test`. These tests also pass the returned totals through `KpiFormatter` and check the exact display strings in the acceptance criteria.
  2. **`KpiFormatter`**: pure unit tests, with no database.
  - There are no controller, view or HTTP tests, as in PBI 4. The nav link and the tile layout are checked by hand.
- **Test-first:** each ticket is built test-first with its own tests and adds its results to the dashboard verification report.
- **Database tests**, in the existing "Test database" collection (it must stay a single collection, because a second one would run in parallel and empty tables during another test). Each test empties `results` and `campaigns` first:
  1. **Clean file:** import the clean sample CSV through `CsvImportService`.
     - Expected totals: spend 45,256.13, revenue 82,078.31, leads 1,630, impressions 4,995,183, clicks 37,882, and ROI ≈ 0.8136.
     - Expected strings: $45,256.13, $82,078.31, 1,630, 4,995,183, 37,882 and 81.4%.
  2. **Empty tables:**
     - Expected totals: all 0, with ROI `null`.
     - Expected strings: $0.00, $0.00, 0, 0, 0 and "N/A (spend is 0)".
     - The result shows there are no Daily Results.
  3. **Zero total spend:** a small inline CSV where every row has spend 0 and some revenue.
     - Expected: ROI `null`, the string "N/A (spend is 0)", and the real revenue total.
  4. **Overall ROI uses totals:** a small inline CSV with one Campaign spending $100 and earning $300, and another spending $10,000 and earning $5,000.
     - Expected: -47.5%, not the +75% average of their ROIs.
  5. **Database failure:** build the service with a connection string to a port where nothing listens (e.g. port 1, 2-second timeout).
     - Expected: an error result with no totals. This test touches no database.
- **Formatter tests:**

  | Input | Expected text |
  |---|---|
  | ROI 0.813640 | 81.4% |
  | ROI 0.81350 (a midpoint) | 81.4% |
  | ROI −0.12345 | -12.3% |
  | ROI −0.0004 | 0.0% |
  | ROI `null` | N/A (spend is 0) |
  | money 1234567.5 | $1,234,567.50 |
  | money 0 | $0.00 |
  | count 0 | 0 |
  | count 4995183 | 4,995,183 |
- **Verification report:** change `VerificationReport` so that each test is tagged with its feature folder, and one report is written per folder. The dashboard tests write `verification-report-tests.md` under `.scratch/dashboard-kpi-tiles/`. The CSV import tests keep writing theirs under `.scratch/csv-import/`, unchanged. After the change, run the **full** test suite and confirm both reports were written (the CSV import report with a new run time). That ticket isn't done until both are checked.
- **Manual check against `campaign_analytics`:**
  - Only the developer touches the real database.
  - The agent prepares `manual-kpi-queries.sql`, with one separate SELECT per tile (deliberately not the app's combined query) plus an ROI query rounded to one decimal, and a manual verification report template.
  - Order of steps:
    1. The developer runs a read-only row count.
    2. If the clean file isn't the only data loaded, the developer gives separate, explicit approval to empty `results` and `campaigns` (Channels are not touched).
    3. The developer opens `/Dashboard` and checks the empty state by eye: the hint with its Import CSV link, $0.00 / 0 tiles and "N/A (spend is 0)".
    4. The developer re-imports the clean sample CSV through the Import CSV page.
    5. The developer runs the manual queries and compares each with its tile.
    6. After the manual check, the developer writes `dashboard-tiles-creation-notes.md`. The agent never writes it.
  - The figures are recorded in `verification-report-manual.md`.
  - The zero-spend and totals-not-average cases are covered only by the automated tests on the test database. No made-up rows go into the real database.
- **Prior art:** the PBI 4 integration tests (`CsvImportServiceTests` with the `TestDatabase` fixture, its database-name guard, and the actual-against-expected `VerificationReport`), and PBI 4 ticket 06's developer-run final check against `campaign_analytics`.

## Out of Scope

- Date, Channel or Campaign filters, and per-Campaign or per-Channel breakdowns.
- Charts, trends and comparisons with earlier periods.
- ROAS, ROI Goals, Weak Campaign alerts, and green/red performance colours.
- Changing the Home page or redirecting `/` to the dashboard.
- Controller, view or HTTP automated tests.
- Schema changes.
- `dashboard-tiles-creation-notes.md`, the PBI's knowledge artifact (the developer writes it).
- The PBI 5 entry in `agent-log.md` (the developer writes it).
- Commits, GitHub issues and pull requests (the developer handles all of these).

## Further Notes

- **Expected figures:** the clean sample CSV's totals, summed straight from the file, match the acceptance criteria exactly: spend 45,256.13, revenue 82,078.31, leads 1,630, impressions 4,995,183, clicks 37,882. ROI = (82,078.31 − 45,256.13) ÷ 45,256.13 = 0.813640, which shows as 81.4%. No row in the clean file has zero spend.
- **Current real data:** after PBI 4's final run, `campaign_analytics` probably holds the error file (236 Daily Results), not the clean file, so the manual check starts with a read-only row count.
- **Permissions:** ask before any install and before any change to `campaign_analytics`.
- **Deliverables:** leave everything uncommitted. The developer commits in past tense, referencing #9.

## PBI Acceptance Criteria (verbatim from issue #9)

- The dashboard link in the navigation bar opens the dashboard page.
- With the clean sample CSV file imported, the spend tile displays $45,256.13, the revenue tile displays $82,078.31, the leads tile displays 1,630, the impressions tile displays 4,995,183, and the clicks tile displays 37,882.
- The ROI tile displays 81.4% as calculated from (revenue - spend) / spend.
- If total spend is 0, the ROI tile displays "N/A (spend is 0)".
- If the results table is empty, spend and revenue are displayed as $0.00, and leads, impressions, and clicks as 0. ROI displays as "N/A (spend is 0)".
- dashboard-tiles-creation-notes.md is committed to the repository root and states how each KPI tile is derived, formatted, and verified manually.
