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


## PBI 6 Agent log

**Tool:** 
Claude Agentic Coding was used to generate an ROI bar chart per channel under the KPI tiles in the dashboard view. Additionally, it was used to generate ROI percentage labels per bar.

**Process used:** 
The grill-with-docs process was used to turn PBI 6 into a spec and four tickets (the 4th ticket was a manual check and a data flow diagram reserved for the developer). This was followed by a ticket-by-ticket implementation that created a ROI by channel bar chart under the KPI tiles in the Dashboard view, sorted bars from highest to lowest ROI, scaled bar width and size, and bar chart error handling for left-out channels, the empty state and database errors.

**What it generated:** 

Spec and tickets:
- spec.md: the PBI 6 spec.
- issues/01-tracer-bullet-roi-bars-for-clean-csv.md
- issues/02-sorting-scaling-and-negative-bars.md
- issues/03-left-out-channels-empty-state-and-error.md
- issues/04-final-manual-check-and-data-flow-diagram.md
- manual-roi-queries.sql
- verification-report-tests.md
- verification-report-manual.md
- images/emptyroibychannel.png and images/roibychannelchartclean.png

App Code:
- Services/DashboardService.cs
- Services/RoiChart.cs 
- Models/DashboardResult.cs
- Controllers/DashboardController.cs: calls the new GetDashboardAsync().
- Views/Dashboard/Index.cshtml
Tests:
- RoiByChannelTests.cs 
- RoiChartTests.cs 
- DashboardServiceTests.cs
- VerificationReport.cs

**How the ROI by channel bar chart creation was checked:** 
- Automatic tests ran on a test copy of the database and did the following:
    - Checked that chart displayed the correct numbers and in the right order
    - Checked error-handling for empty state, individual channel with no spend, and channel losing money cases
- Manual tests ran by the developer and did the following:
    - Emptied campaign_analytics database and verified an empty bar chart with an option to "Import a CSV"
    - Imported the clean sample CSV file and saw the four bars per channel with the correct percentages: Email 155.1%, Search 131.4%, Display 46.4%, Social 24.8%.
    - Ran manual SQL queries that verified the percentages were correct

**Decisions made:** 
- Each channel's ROI comes from its total revenue and total spend. It is never derived from the average of its campaigns' ROIs, as that could distort the final results.
- The bars are sorted from highest ROI % to lowest ROI %. If two channels have the same ROI %, they're sorted alphabetically (A-Z).
- The bars are displayed horizontally and under the KPI tiles. 
- Channels with a positive ROI % grow to the right, while channels with a negative ROI % grow to the left.
- A channel with no spend is left out of the bar chart, and a note explaining its absence is displayed to the user.
- No data imported prompts the user to "Import a CSV" with a link to do so.
- Data imported but no spend across any of the channels displays "No channel has any spend yet" to the user.
- Database failures prompts and displays an error message.
- scratch/roi-by-channel-bar-chart is committed as proof of the process.

**What was learned:** 
- The clean sample data CSV file does not account for edge cases like an individual channel having no spend, a negative ROI, an empty results table/no data at all, or database failures. Going through the grill-me-process helped unveil these cases, and implement error-handling tests that prevent the dashboard from crashing if future cases do occur.
- Choosing HTML and CSS bars over a chart library ensured that every label was text that could be checked against SQL.
- Keeping calculations out of the view ensures that the math can be reached by the automated tests. It's better to do calculations in testable code, and let the page only display the results of that code. In this PBI, the logic was moved into RoiChart instead of keeping it in the view.


## PBI 7 Agent log

**Tool:** 
Claude Agentic Coding was used to generate a users SQL table, password hashing, seeded marketer and admin users, a login page with username and password inputs, cookie authentication, protected Dashboard and Import CSV pages, a nav bar displaying the user logged in, a log out link, error-handling for empty or invalid username and password submissions, error-handling for an unavailable database, and automated tests.

**Process used:** 
The grill-with-docs process was used to turn PBI 7 into a spec and four tickets (the 4th ticket was a manual check and a log in, log out sequence diagram reserved for the developer). This was followed by a ticket-by-ticket implementation process that was verified one at a time. This created a users table in campaign_analytics_test databases after the developer approved the SQL script, as well as code that created a login page with username and password inputs, cookie authentication, protected Dashboard and Import CSV pages, a nav bar displaying the user logged in, a log out link, error-handling for empty or invalid username and password submissions, error-handling for an unavailable database, and automated tests.

**What it generated:** 

Spec and tickets:
- spec.md: the PBI 7 spec.
- issues/01-tracer-bullet-seeded-user-logs-in-to-dashboard.md
- issues/02-login-errors-and-edge-cases.md
- issues/03-log-out-nav-bar-and-session-safety.md
- issues/04-final-manual-check.md
- create-users-table.sql: creates the users table.
- verification-report-tests.md
- verification-report-manual.md
- images/MarketerLoginLandingPage.png, images/AdminLoginLandingPage.png, images/EmptyUserAndPassCheck.png, images/InvalidUserOrPassCheck.png, images/NotLoggedInDashboardCheck.png, images/NotLoggedInImportCheck.png, images/PasswordHashSQLCheck.png and images/LoggedOut.png

App Code:
- Services/UserService.cs: logs Users in and seeds the Marketer and Admin.
- Models/LogInForm.cs
- Models/LogInPage.cs
- Models/LogInResult.cs
- Models/Role.cs
- Controllers/AccountController.cs: the login page, login submit, and log out.
- Controllers/DashboardController.cs
- Controllers/ImportController.cs
- Controllers/RejectOversizedImportAttribute.cs
- Program.cs: cookie authentication, 30-minute timeout, and seeding at startup.
- Views/Account/Login.cshtml
- Views/Shared/_Layout.cshtml

Tests:
- UserServiceTests.cs
- ControllerAccessTests.cs
- TestDatabase.cs
- VerificationReport.cs: adds the login-logout report.


**How the ROI by channel bar chart creation was checked:** 
- Automatic tests ran on a test copy of the database and did the following:
    - Checked that the right password loggged both the marketer and admin accounts
    - Ensured that a wrong password, an unknown username, a password in different capitalization were all refused
    - Ensured that empty username or password inputs were refused
    - Ensured that if the database was down, a user would get Unavailable instead of a crash
    - 
    - Checked that the Dashboard and Import pages were protected and unavailable if a user was not logged in

- Manual tests ran by the developer and did the following:
    - Ran manual SQL query (DESCRIBE users;) to check that four fields (user_id, username, password_hash, and role) were in users table
    - Ran manual SQL query (SELECT) to check that the two users, marketer and admin, were under the correct roles
    - Checked that users, marketer and admin, could log in and land on the Dashboard
    - Checked that a wrong password or an unknown username showed message "Invalid username or password"
    - Checked that empty input showed "Enter a username and password"
    - Checked that, while logged out, the Dashboard and Import pages were unavailable and the user would land at login page
    - Checked that opening the login page while logged in landed on the Dashboard page
    - Checked that field, password_hash, held hashes and not plain-text passwords

**Decisions made:** 
- ASP.NET Core gives two options for a login system: ASP.NET Core Identity and Cookie Authentication. Due to only having two Users in the database and the small size of the app, Cookie Authentication with the users table was chosen to avoid out-of-scope features such as a registration page, a password reset, and a lockout. Additionally, ASP.NET Core Identity uses its own tables and EF Core, which did not fit (without installing new packages, using Identity's tables, storing Roles in separate tables, and creating tables with EF Core migrations) with the current users table and the plain SQL that the app uses.
- Passwords are never saved as plain text, but instead are turned and saved into hashes. This ensures the database protects them and does not easily reveal them.
- There are only two Users, marketer and admin. Their passwords are kept in User Secrets, not in the code.
- The Dashboard and Import CSV pages need a login in order to access them, otherwise they are protected.
- Pressing Back does not save a protected page after a user logs out.
- Roles are saved but do not do anything yet. Roles will have more of a function in PBI 8.
- scratch/login-logout is committed as proof of the process.

**What was learned:** 
- Pressing back after logging out could still give an unauthorized user a view of the protected pages (Dashboard and Import CSV). It was learned that setting a no-store fix would tell the browser to not save the protected pages.
    - One page was missed at first: the "file too large" error page is made by a filter before the controller runs, so it needed the same fix added on its own. It was learned here that a rule on a controller only covers requests that actually reach the controller. If a filter stops the request, the rule must be added to the filter too.


