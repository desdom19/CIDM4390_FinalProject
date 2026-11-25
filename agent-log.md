## PBI 3 Agent log

**Tool:** 
Claude Agentic Coding was used to generate both the clean and error CSVs. 

**Process used:** 
The grill-with-docs process was used to turn PBI 3 into a spec and two tickets. This was followed by the implementation that generated the sample data in both CSVs.

**What it generated:** 
- campaign_results_clean.csv
- campaign_results_errors.csv
- spec.md 
- verification-report-clean.md
- verification-report-errors.md
- .scratch/sample-campaign-data/
- issues/01-clean-campaign-results-csv.md
- issues/02-error-campaign-results-csv.md

**How the data generation was checked:** 
- Functions such as "(Import-Csv sample-data\campaign_results_errors.csv).Count" were used to verify correct row counts. Defect lines were verified manually by opening the file and using CTRL+G to find the defect lines.

**Decisions made:** 
- The agent flagged a mismatch during the grill-with-docs process. The sample data was being built around revenue / spend (ROAS) but the schema column is roi_goal (ROI). The agent’s suggestion was to treat roi_goal as a ROAS target. If left unresolved, the future dashboard could compare a ROAS value against an ROI goal. I chose to keep roi_goal as true ROI ((revenue - spend) / spend) because it matched the column name and the planned dashboard formula.
- .scratch/sample-campaign-data/ will be committed as proof of the process.

**What was learned:** 
- The difference between an ROI and ROAS. While a ROAS is measured by dividing revenue / spend, an ROI is measured by subtracting spend from revenue then dividing by spend. A ROAS measures revenue from a more isolated standpoint, only taking into account ad spend in relation to it. In general, an ROI measures profit by accounting for costs beyond ad spend, but in this project it uses only ad spend. Put another way, ROI = ROAS - 1.  


## PBI 4 Agent log

**Tool:** 
Claude Agentic Coding was used to implement a CSV upload and import feature that validates each row, skips duplicate results, and inserts valid rows into the MySQL database campaign_analytics. 

**Process used:** 
The grill-with-docs process was used to turn PBI 4 into a spec and 7 tickets (the 6th ticket is a manual check confirmed by the developer). Additionally, 12 automated xUnit tests were created and run against a test database, campaign_analytics_test.

**What it generated:** 

Spec and tickets:
- spec.md: the PBI 4 spec. 
- issues/01-tracer-bullet-import-clean-csv.md
- issues/02-row-validation-and-rejected-rows.md
- issues/03-duplicate-daily-results.md
- issues/04-file-level-rejections.md
- issues/05-channel-mismatch-rejection.md 
- issues/06-final-manual-run-campaign-analytics.md
- issues/07-automated-tests-against-test-database.md
- setup-test-database.sql: created the campaign_analytics_test database.
- verification-report-tests.md: written automatically by each test run.


App Code:
- Services/CsvImportService.cs: the import itself. It parses the file with CsvHelper, validates each row, finds or creates Campaigns, skips duplicates, and writes everything in one transaction.
- Controllers/ImportController.cs: the upload page and form handling.
- Controllers/RejectOversizedImportAttribute.cs: enforces the 5 MB limit.
- Models/ImportSummary.cs: the summary counts and the rejected rows.
- Views/Import/Index.cshtml: the upload form, summary and rejected rows table.
- Views/Shared/_Layout.cshtml: adds the "Import CSV" nav link.
- Program.cs
- CampaignPerformanceAnalytics.csproj

Tests:
- CsvImportServiceTests.cs: 12 tests.
- TestDatabase.cs:
- VerificationReport.cs
- RepositoryPaths.cs
- CampaignPerformanceAnalytics.Tests.csproj

**How the CSV imports were checked:** 
- CampaignPerformanceAnalytics.Tests runs 12 automated xUnit tests against the campaign_analytics_test database that cover:
- The clean CSV imports 240 rows and leaves 8 Campaigns dated 2026-09-01 to 2026-09-30.
- Re-importing the clean CSV imports 0 and skips 240 as duplicates.
- The error CSV imports 236, rejects 4 (lines 45, 126, 169, 221) and skips 1 duplicate (line 202).
- All 12 passed ended up passing.
- Outside of the 12 tests, the developer (I) manually uploaded the clean sample CSV file and error sample CSV file, ensuring that they gave the correct import results, rejection, and duplicate numbers.
- SQL counts were utilized manually after each upload.

**Decisions made:** 
- Matching follows MySQL’s case-insensitive collation for campaigns and channels.
- Tests were moved to the last ticket so the working import came first.
- Skipping duplicates instead of updating them.
- Testing against a separate database.
- No plausability checks.
- .scratch/csv-import/ will be committed as proof of the process.


**What was learned:** 
- How CsvHelper works such as how it handles the parsing (e.g., quoted fields, commas insider values, mapping headers to columns).
- Validation vs. plausability. Validation checks are utilized to verify if values are correct according to the database field rules (e.g., date being in yyyy-MM-dd order), while plausability is used to check whether the values make sense in the context they're in within the database. Plausability issues were not implemented in this PBI to draw a line on what the import's job is: reject broken data, not judge the data. 
- A database transaction is a group of database actions that makes sure multi-step changes occur all at once, or never at all. For example, if the database fails, a transaction prevents the file from being half-loaded in the database, and instead undoes everything already done in the transaction (rollback). A rollback allows a user to simply upload the file again instead of wondering up to how much of the file was loaded. 

## PBI 5 Agent log

**Tool:** 
Claude Agentic Coding was used to generate a dashboard page, a dashboard link in the navigation bar, five total KPI tiles (spend, revenue, leads, impressions, clicks), and an ROI tile. Addititionally, agentic coding was utilized to formulate calculations on each of the KPI tiles.

**Process used:** 
The grill-with-docs process was used to turn PBI 5 into a spec and four tickets (the 4th ticket was a manual check reserved for the developer). This was followed by the implementation that generated the dashboard page, dashboard link, five total KPI tiles, the ROI tile, and the calculations that went into each KPI tile.

**What it generated:** 

Spec and tickets:
- spec.md: the PBI 5 spec.
- issues/01-tracer-bullet-five-total-tiles.md
- issues/02-overall-roi-tile.md
- issues/03-empty-and-database-failure-states.md
- issues/04-final-manual-check-and-notes.md
- manual-kpi-queries.sql
- verification-report-tests.md: written automatically by each test run.
- verification-report-manual.md: the figures from the manual check against campaign_analytics.

App Code:
- Services/DashboardService.cs: reads the five totals (spend, revenue, leads, impressions, clicks) from the results table in one SQL query, and returns an error result if the database can't be reached.
- Services/KpiFormatter.cs: turns the figures into tile text ($45,256.13, 4,995,183, 81.4%, "N/A (spend is 0)").
- Controllers/DashboardController.cs: the /Dashboard page.
- Models/DashboardResult.cs: the totals or an error message, and KpiTotals, where Overall ROI is calculated.
- Views/Dashboard/Index.cshtml: the six KPI tiles, the empty-state hint and the database error message.
- Views/Shared/_Layout.cshtml: adds the "Dashboard" nav link between Home and Import CSV.
- Program.cs

Tests:
- DashboardServiceTests.cs: 5 tests.
- KpiFormatterTests.cs: 9 tests.
- VerificationReport.cs: now writes one report per feature folder.
- TestDatabase.cs
- CsvImportServiceTests.cs: tagged with its feature folder so its report stays under .scratch/csv-import/.

**How the dashboard tile creation was checked:** 
- Automated tests against campaign_analytics_test where the clean file gave the five totals, and the empty tables and zero-spend cases gave 0 and N/A (spend is 0).
- An automated test that proved overall ROI used totals, not an average of the ROIs.
- An automated formatter test that covered rounding, negatives, and 0.0%.
- A manual test that checked the row count first, and emptied the tables. This test had the developer verify that the empty dashboard had the appropriate tile displays in place (e.g., $0.00, 0, N/A (spend is 0)).
- A manual test that re-imported the clean file and compared each tile with separate SQL queries.

**Decisions made:** 
- The overall ROI being derived from totals, not an average of campaign ROIs. Deriving overall ROI from an average of campaign ROIs gives a campaign with less spend the same importance as one with higher spend, which distorts the result. 
- Generating the dashboard to display totals for all campaigns instead of being able to filter to one specific campaign. This filter is saved for a future PBI.
- Using one SELECT SQL query instead of five separate ones for each KPI tiles (excluding ROI).
- scratch/dashboard-kpi-tiles is committed as proof of the process.

**What was learned:** 
- In order to keep results proportionate to how the money was actually spent, it's better to derive overall ROI from totals instead of the average of campaign ROIs. This prevents a small campaign with a good ROI to hide a big campaign that is losing money.
- Invariant culture prevents tests comparing exact values to not break.

